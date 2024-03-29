﻿using EzAspDotNet.HttpClient;
using EzAspDotNet.Models;
using EzMongoDb.Util;
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

namespace LolCrawler.Api
{
    public partial class RiotCrawler
    {
        public string ApiKey { get; set; }

        public RiotApi RiotApi { get; private set; }

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

            MongoDbCurrentGame.Collection.Indexes.CreateMany(new List<CreateIndexModel<CurrentGame>>
            {
                new CreateIndexModel<CurrentGame>(Builders<CurrentGame>.IndexKeys.Ascending(x => x.GameId),
                               new CreateIndexOptions { Unique = true })
            });

            MongoDbMatch.Collection.Indexes.CreateMany(new List<CreateIndexModel<Match>>
            {
                new CreateIndexModel<Match>(Builders<Match>.IndexKeys.Ascending(x => x.GameId),
                               new CreateIndexOptions { Unique = true })
            });

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

        private static FilterDefinition<Summoner> SummonerFilter(string summonerName, Region region)
        {
            return Builders<Summoner>.Filter.Eq(x => x.NameLower, summonerName.ToLower()) &
                    Builders<Summoner>.Filter.Eq(x => x.Region, region.Key);
        }

        private static FilterDefinition<Summoner> SummonerIdFilter(string summonerId, Region region)
        {
            return Builders<Summoner>.Filter.Eq(x => x.SummonerId, summonerId) &
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
                Log.Logger.Error("<Exception:{ExMessage}>", ex.Message);
                return null;
            }
        }

        public async Task<Summoner> RefreshSummoner(string summonerName, Region region, bool tracking)
        {
            var summoner = await RiotApi.SummonerV4.GetBySummonerNameAsync(region, summonerName);
            if (summoner == null)
            {
                return null;
            }

            await RefreshLeagueEntries(summoner.Id, region);
            return await SummonerUpsert(await MongoDbSummoner.FindOneAsync(SummonerFilter(summonerName, region)),
                summoner, region, tracking);
        }

        public async Task<Summoner> RefreshSummonerById(string summonerId, Region region, bool tracking)
        {
            var summoner = await RiotApi.SummonerV4.GetBySummonerIdAsync(region, summonerId);
            if (summoner == null)
            {
                return null;
            }

            await RefreshLeagueEntries(summoner.Id, region);
            return await SummonerUpsert(await MongoDbSummoner.FindOneAsync(SummonerIdFilter(summonerId, region)),
                summoner, region, tracking);
        }

        private async Task<Summoner> SummonerUpsert(Summoner summonerData,
            MingweiSamuel.Camille.SummonerV4.Summoner summoner, Region region, bool tracking)
        {
            if (summonerData == null)
            {
                summonerData = new Summoner
                {
                    Name = summoner.Name,
                    NameLower = summoner.Name.ToLower(),
                    Level = summoner.SummonerLevel,
                    AccountId = summoner.AccountId,
                    Puuid = summoner.Puuid,
                    Region = region.Key,
                    SummonerId = summoner.Id,
                    Tracking = tracking
                };
                await MongoDbSummoner.CreateAsync(summonerData);
            }
            else
            {
                summonerData.Name = summoner.Name;
                summonerData.NameLower = summoner.Name.ToLower();
                summonerData.Level = summoner.SummonerLevel;
                summonerData.Tracking = tracking;
                await MongoDbSummoner.UpdateAsync(summonerData.Id, summonerData);
            }
            return summonerData;
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

                return await RefreshSummoner(summonerName, region, tracking);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("<Exception:{ExMessage}>", ex.Message);
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

        private async Task<List<LeagueEntry>> RefreshLeagueEntries(string summonerId, Region region)
        {
            try
            {
                if (MapperUtil.Mapper == null)
                    return null;

                var leagueEntriesOrigin = await RiotApi.LeagueV4.GetLeagueEntriesForSummonerAsync(region, summonerId);

                if (leagueEntriesOrigin == null)
                    return null;

                var leagueEntries = leagueEntriesOrigin
                    .Select(MapperUtil.Map<LeagueEntry>)
                    .ToList();

                foreach (var leagueEntry in leagueEntries)
                {
                    await MongoDbLeagueEntry.UpsertAsync(Builders<LeagueEntry>.Filter.Eq(x => x.SummonerId, leagueEntry.SummonerId) &
                                                         Builders<LeagueEntry>.Filter.Eq(x => x.QueueType, leagueEntry.QueueType), leagueEntry);
                }

                return leagueEntries;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("<Exception:{ExMessage}>", ex.Message);
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
                Log.Logger.Error("<Exception:{ExMessage}>", ex.Message);
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
                Log.Logger.Error("<Exception:{ExMessage}>", ex.Message);
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
                    var playingGame = await MongoDbCurrentGame.FindOneAsync(Builders<CurrentGame>.Filter.Eq(x => x.GameId, summoner.TrackingGameId.GetValueOrDefault()));
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

                // 사용자 정의 게임은 종료 처리가 기록되지 않는다. lol-crawler는 결과 전달을 위해 진행 상태인 게임을 추적하므로 이러한 게임 타입을 필터링한다.
                if (currentGame.GameType == "CUSTOM_GAME")
                {
                    return null;
                }

                var origin = await MongoDbCurrentGame.FindOneAsync(Builders<CurrentGame>.Filter.Eq(x => x.GameId, currentGame.GameId));
                if (origin != null)
                {
                    return origin;
                }

                var newCurrentGame = new CurrentGame { GameId = currentGame.GameId, Info = currentGame };

                summoner.TrackingGameId = newCurrentGame.GameId;
                var result = await MongoDbSummoner.UpdateAsync(summoner.Id, summoner);

                var newDbGame = await MongoDbCurrentGame.CreateAsync(newCurrentGame);

                action?.Invoke(newDbGame);
                return newDbGame;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("<Exception:{ExMessage}>", ex.Message);
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
                Log.Logger.Error("<Exception:{ExMessage}>", ex.Message);
                return null;
            }
        }


        public async Task GetMatch(Summoner summoner, long gameId, Region region, Action<Match> action = null)
        {
            try
            {
                var filter = Builders<Match>.Filter.Eq(x => x.GameId, gameId);
                var match = await MongoDbMatch.FindOneAsync(filter);
                if (match != null)
                    return;

                var matchRegion = MatchRegion.FromRegion(region);
                var matchData = await RiotApi.MatchV5.GetMatchAsync(matchRegion, $"{region.Key}_{gameId}");
                if (matchData == null)
                    return;

                await MongoDbMatch.UpsertAsync(filter,
                    MapperUtil.Map<Match>(matchData),
                    (newMatch) =>
                    {
                        action?.Invoke(newMatch);
                    });
            }
            catch (Exception ex)
            {
                Log.Logger.Error("<Exception:{ExMessage}>", ex.Message);
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

                if (cultureInfo == null) return null;

                var cultureCode = cultureInfo.Name.Replace("-", "_");

                var championList = await HttpClient.Request<ChampionList>(HttpMethod.Get, $"http://ddragon.leagueoflegends.com/cdn/{version}/data/{cultureCode}/champion.json");

                var originChampions = (await MongoDbChampion.All()).ToDictionary(x => x.ChampionId);

                var champions = championList.Data.Values.ToList();
                foreach (var champion in champions.Where(champion => !originChampions.ContainsKey(champion.ChampionId)))
                {
                    await MongoDbChampion.UpsertAsync(Builders<Models.Champion>.Filter.Eq(x => x.ChampionId, champion.ChampionId), champion);
                }

                return champions;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("<Exception:{ExMessage}>", ex.Message);
                return null;
            }
        }
    }
}
