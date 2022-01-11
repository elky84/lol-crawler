using LolCrawler.Code;
using LolCrawler.Models;
using LolCrawler.Protocols;
using MingweiSamuel.Camille;
using MingweiSamuel.Camille.Enums;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDbWebUtil.Util;
using EzAspDotNet.HttpClient;
using EzAspDotNet.Util;

namespace LolCrawler.Api
{
    public partial class RiotCrawler
    {
        public string ApiKey { get; set; }

        public RiotApi RiotApi { get; set; }

        private MongoDbUtil<Summoner> MongoDbSummoner { get; set; }

        private MongoDbUtil<CurrentGame> MongoDbCurrentGame { get; set; }

        private MongoDbUtil<Match> MongoDbMatch { get; set; }

        private MongoDbUtil<Models.Champion> MongoDbChampion { get; set; }

        private MongoDbUtil<LeagueEntry> MongoDbLeagueEntry { get; set; }

        private HttpClient HttpClient { get; set; }

        public RiotCrawler(IMongoDatabase mongoDatabase, HttpClient httpClient)
        {
            MongoDbSummoner = new MongoDbUtil<Summoner>(mongoDatabase);
            MongoDbCurrentGame = new MongoDbUtil<CurrentGame>(mongoDatabase);
            MongoDbMatch = new MongoDbUtil<Match>(mongoDatabase);
            MongoDbChampion = new MongoDbUtil<Models.Champion>(mongoDatabase);
            MongoDbLeagueEntry = new MongoDbUtil<LeagueEntry>(mongoDatabase);

            HttpClient = httpClient;
        }

        public RiotCrawler Create(string apiKey, int maxConcurrentRequests = 5, int retries = 0)
        {
            this.ApiKey = apiKey;
            RiotApi = RiotApi.NewInstance(new RiotApiConfig.Builder(apiKey)
            {
                MaxConcurrentRequests = maxConcurrentRequests,
                Retries = retries
            }.Build());
            return this;
        }

        private FilterDefinition<Summoner> SummonerFilter(string summonerName, Region region)
        {
            return Builders<Summoner>.Filter.Eq(x => x.NameLower, summonerName.ToLower()) &
                    Builders<Summoner>.Filter.Eq(x => x.Region, region.Key);
        }

        public async Task<Summoner> GetSummerByName(string summonerName, Region region)
        {
            try
            {
                return await MongoDbSummoner.FindOneAsync(SummonerFilter(summonerName, region));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }
        }

        public async Task<Summoner> CreateSummerByName(string summonerName, Region region, bool tracking)
        {
            try
            {
                var origin = await GetSummerByName(summonerName, region);
                if (origin != null)
                {
                    return origin;
                }

                var summoner = await RiotApi.SummonerV4.GetBySummonerNameAsync(region, summonerName);
                if (summoner == null)
                {
                    return null;
                }

                await RefreshLeagueEntries(summoner.Id, region);

                return await MongoDbSummoner.UpsertAsync(SummonerFilter(summonerName, region),
                    new Summoner
                    {
                        Name = summoner.Name,
                        NameLower = summoner.Name.ToLower(),
                        Level = summoner.SummonerLevel,
                        AccountId = summoner.AccountId,
                        Puuid = summoner.Puuid,
                        Region = region.Key,
                        SummonerId = summoner.Id,
                        Tracking = tracking
                    });
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }
        }

        public async Task<List<LeagueEntry>> GetLeagueEntries(Summoner summoner)
        {
            if (summoner == null)
            {
                return new List<LeagueEntry>();
            }

            return await MongoDbLeagueEntry.FindAsync(Builders<LeagueEntry>.Filter.Eq(x => x.SummonerId, summoner.SummonerId));
        }

        public async Task<bool> DeleteLeagueEntries(Summoner summoner)
        {
            if (summoner == null)
            {
                return false;
            }

            return await MongoDbLeagueEntry.RemoveManyAsync(Builders<LeagueEntry>.Filter.Eq(x => x.SummonerId, summoner.SummonerId));
        }

        public async Task<List<LeagueEntry>> RefreshLeagueEntries(Summoner summoner)
        {
            if (summoner == null)
            {
                return new List<LeagueEntry>();
            }

            return await RefreshLeagueEntries(summoner.SummonerId, Region.Get(summoner.Region));
        }

        public async Task<List<LeagueEntry>> RefreshLeagueEntries(string summonerId, Region region)
        {
            try
            {
                var leagueEntires = (await RiotApi.LeagueV4.GetLeagueEntriesForSummonerAsync(region, summonerId)).Select(x => x.ConvertTo<LeagueEntry, MingweiSamuel.Camille.LeagueV4.LeagueEntry>()).ToList();
                foreach (var leagueEntry in leagueEntires)
                {
                    await MongoDbLeagueEntry.UpsertAsync(Builders<LeagueEntry>.Filter.Eq(x => x.SummonerId, leagueEntry.SummonerId) &
                        Builders<LeagueEntry>.Filter.Eq(x => x.QueueType, leagueEntry.QueueType), leagueEntry);
                }

                return leagueEntires;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }
        }

        public async Task<Summoner> UpdateSummerByName(string summonerName, Region region, bool tracking)
        {
            try
            {
                var origin = await GetSummerByName(summonerName, region);
                if (origin == null)
                {
                    return null;
                }

                origin.Tracking = tracking;
                await MongoDbSummoner.UpdateAsync(SummonerFilter(summonerName, region), origin);
                return origin;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }
        }

        public async Task<Summoner> DeleteSummerByName(string summonerName, Region region)
        {
            try
            {
                return await MongoDbSummoner.RemoveGetAsync(SummonerFilter(summonerName, region));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }
        }

        public async Task<List<Summoner>> GetTrackingSummoners()
        {
            return await MongoDbSummoner.FindAsync(Builders<Summoner>.Filter.Eq(x => x.Tracking, true));
        }


        public async Task<CurrentGame> GetCurrentGame(Summoner summoner, Action<CurrentGame> action = null)
        {
            try
            {
                if (summoner.TrackingGameId.HasValue)
                {
                    var playingGame = await MongoDbCurrentGame.FindOneAsync(Builders<CurrentGame>.Filter.Eq(x => x.Info.GameId, summoner.TrackingGameId.GetValueOrDefault()));
                    if (playingGame != null)
                    {
                        return playingGame;
                    }
                }

                var currentGame = await RiotApi.SpectatorV4.GetCurrentGameInfoBySummonerAsync(Region.Get(summoner.Region), summoner.SummonerId);
                if (currentGame == null)
                {
                    return null;
                }

                var origin = await MongoDbCurrentGame.FindOneAsync(Builders<CurrentGame>.Filter.Eq(x => x.Info.GameId, currentGame.GameId));
                if(origin != null)
                {
                    return origin;
                }

                var newCurrentGame = new CurrentGame { Info = currentGame };

                summoner.TrackingGameId = newCurrentGame.Info.GameId;
                await MongoDbSummoner.UpdateAsync(summoner.Id, summoner);

                var newDbGame = await MongoDbCurrentGame.CreateAsync(newCurrentGame);

                action?.Invoke(newDbGame);
                return newDbGame;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }
        }

        public CurrentGame EndGame(Summoner summoner, CurrentGame currentGame)
        {
            try
            {
                summoner.TrackingGameId = null;
                MongoDbSummoner.Update(summoner.Id, summoner);

                currentGame.GameState = Code.GameState.End;
                return MongoDbCurrentGame.Update(currentGame.Id, currentGame);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }
        }


        public async Task GetMatch(Summoner summoner, long gameId, Region region, Action<Match> action = null)
        {
            try
            {
                var match = await MongoDbMatch.FindOneAsync(Builders<Match>.Filter.Eq(x => x.Info.GameId, gameId));
                if (match != null)
                {
                    return;
                }

                var matchRegion = MatchRegion.FromRegion(region);
                var matchIds = await RiotApi.MatchV5.GetMatchIdsByPUUIDAsync(matchRegion, summoner.Puuid);
                List<Match> matches = new();
                foreach(var matchId in matchIds)
                {
                    // 현재 게임에 대한 matchMetadata가 있으면 디스코드 알림하고, 현재 게임 정보 상태를 바꾸고, 폴링 대상에서 제거
                    var matchData = await RiotApi.MatchV5.GetMatchAsync(matchRegion, matchId);
                    if (matchData == null)
                    {
                        Log.Logger.Error($"Not found MatchData. <Region:{matchRegion}> <MatchId:{matchId}>");
                        continue;
                    }

                    matches.Add(await MongoDbMatch.UpsertAsync(Builders<Match>.Filter.Eq(x => x.Info.GameId, gameId),
                        matchData.ConvertTo<Match, MingweiSamuel.Camille.MatchV5.Match>(),
                        (match) =>
                        {
                            if( match.Info.GameId == gameId)
                            {
                                action?.Invoke(match);
                            }
                        }));
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return;
            }

        }


        public async Task<List<Models.Champion>> GetChampions(Region region)
        {
            try
            {
                var regionInfo = new RegionInfo(region.Key);

                var cultureInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                    .FirstOrDefault(x => x.ThreeLetterWindowsLanguageName.ToLower() == regionInfo.ThreeLetterWindowsRegionName.ToLower());

                var version = (await HttpClient.Request<List<string>>(HttpMethod.Get, "https://ddragon.leagueoflegends.com/api/versions.json")).FirstOrDefault();

                var cultureCode = cultureInfo.Name.Replace("-", "_");

                var championList = await HttpClient.Request<ChampionList>(HttpMethod.Get, $"http://ddragon.leagueoflegends.com/cdn/{version}/data/{cultureCode}/champion.json");

                var originChampions = (await MongoDbChampion.All()).ToDictionary(x => x.ChampionId);

                var champions = championList.Data.Values.ToList();
                foreach (var champion in champions)
                {
                    if (!originChampions.ContainsKey(champion.ChampionId))
                    {
                        await MongoDbChampion.UpsertAsync(Builders<Models.Champion>.Filter.Eq(x => x.ChampionId, champion.ChampionId), champion);
                    }
                }

                return champions;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
                return null;
            }

        }
    }
}
