using Server.Models;
using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using LolCrawler.Api;
using MingweiSamuel.Camille.Enums;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MongoDbWebUtil.Settings;
using System;
using Serilog;

namespace Server.Services
{
    public class TrackingService
    {
        private readonly RiotCrawler _riotApiCrawler;

        private readonly NotificationService _notificationService;

        private Dictionary<long, LolCrawler.Models.Champion> _champions = new Dictionary<long, LolCrawler.Models.Champion>();

        private DateTime OldDate;

        public TrackingService(IConfiguration configuration,
            MongoDbService mongoDbService,
            NotificationService notificationService,
            IHttpClientFactory httpClientFactory)
        {
            _riotApiCrawler = new RiotCrawler(mongoDbService.Database, httpClientFactory.CreateClient()).Create(configuration.GetRiotApiCrawlerSettings().RiotApiKey);
            _notificationService = notificationService;
        }

        private async Task LoadChampions()
        {
            var champions = await _riotApiCrawler.GetChampions(Region.KR);
            if (champions != null)
            {
                _champions = champions.ToDictionary(x => x.Key);
            }
        }

        public async Task ExecuteBackground()
        {
            if (OldDate.Date != DateTime.Now.Date)
                await LoadChampions();

            OldDate = DateTime.Now;

            foreach (var summoner in await _riotApiCrawler.GetTrackingSummoners())
            {
                var playingGame = await _riotApiCrawler.GetCurrentGame(summoner,
                    async (game) =>
                    {
                        var participant = game.Info.Participants.FirstOrDefault(x => x.SummonerId == summoner.SummonerId);
                        if(participant == null)
                        {
                            Log.Error($"Not found Participant. <SummonerName:{summoner.Name}> <SummonerId:{summoner.SummonerId}> <GameId:{game.Info.GameId}>");
                            await _notificationService.Execute(summoner.Region, $"{summoner.Name}님이 {game.Info.GameMode} 모드 게임을 시작하셨습니다");
                            return;
                        }

                        var champion = _champions[participant.ChampionId];

                        var message = $"{summoner.Name}님이 {champion.Name}(으)로 {game.Info.GameMode} 모드 게임을 시작하셨습니다";
                        var championImageUrl = $"http://ddragon.leagueoflegends.com/cdn/img/champion/splash/{champion.ChampionId}_0.jpg";
                        await _notificationService.Execute(summoner.Region, message, new List<string> { championImageUrl });
                    });

                if (playingGame != null && playingGame.GameState == LolCrawler.Code.GameState.Playing)
                {
                    await _riotApiCrawler.GetMatch(summoner, playingGame.Info.GameId, Region.Get(summoner.Region),
                        async (match) =>
                        {
                            _riotApiCrawler.EndGame(summoner, playingGame);

                            var participantIdentity = match.Info.Participants.FirstOrDefault(x => x.SummonerId == summoner.SummonerId);
                            if (participantIdentity == null)
                            {
                                Log.Error($"Not found participantIdentity. <SummonerName:{summoner.Name}> <SummonerId:{summoner.SummonerId}> <GameId:{match.Info.GameId}>");
                                await _notificationService.Execute(summoner.Region, $"{summoner.Name}님이 {match.Info.GameMode} 모드 게임을 종료하셨습니다");
                                return;
                            }

                            var participant = match.Info.Participants.FirstOrDefault(x => x.ParticipantId == participantIdentity.ParticipantId);
                            if (participantIdentity == null)
                            {
                                Log.Error($"Not found Participant. <SummonerName:{summoner.Name}> <SummonerId:{summoner.SummonerId}> <GameId:{match.Info.GameId}>");
                                await _notificationService.Execute(summoner.Region, $"{summoner.Name}님이 {match.Info.GameMode} 모드 게임을 종료하셨습니다");
                                return;
                            }

                            var win = participant.Win;
                            var k = participant.Kills;
                            var d = participant.Deaths;
                            var a = participant.Assists;
                            var kda = (k + a) / (float)d;

                            var champion = _champions[participant.ChampionId];
                            var championImageUrl = $"http://ddragon.leagueoflegends.com/cdn/img/champion/splash/{champion.ChampionId}_0.jpg";

                            if (win)
                            {
                                var message = $"{summoner.Name}님이 {champion.Name}(으)로 {playingGame.Info.GameMode}모드 게임을 승리하셨습니다. KDA[{kda}, {k}/{d}/{a}]";
                                var winImageUrl = "https://mir-s3-cdn-cf.behance.net/project_modules/1400/c9916f54385211.5959b34077df7.jpg";

                                await _notificationService.Execute(summoner.Region, message, new List<string> { championImageUrl, winImageUrl });
                            }
                            else
                            {
                                var message = $"{summoner.Name}님이 {champion.Name}(으)로 {playingGame.Info.GameMode}모드 게임을 패배하셨습니다. KDA[{kda}, {k}/{d}/{a}]";
                                var loseImageUrl = "https://mir-s3-cdn-cf.behance.net/project_modules/1400/c9ccce54385211.5959b3407819c.jpg";

                                await _notificationService.Execute(summoner.Region, message, new List<string> { championImageUrl, loseImageUrl });
                            }

                            await _riotApiCrawler.RefreshLeagueEntries(summoner.Id, Region.Get(summoner.Region));
                        });
                }
            }
        }
    }
}