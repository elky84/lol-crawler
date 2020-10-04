using Server.Models;
using System.Threading.Tasks;
using WebUtil.Services;
using LolCrawler.Api;
using MingweiSamuel.Camille.Enums;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using WebUtil.Settings;

namespace Server.Services
{
    public class TrackingService
    {
        private readonly RiotCrawler _riotApiCrawler;

        private readonly NotificationService _notificationService;

        private readonly Dictionary<long, LolCrawler.Models.Champion> _champions = new Dictionary<long, LolCrawler.Models.Champion>();

        public TrackingService(IConfiguration configuration,
            MongoDbService mongoDbService,
            NotificationService notificationService,
            IHttpClientFactory httpClientFactory)
        {
            _riotApiCrawler = new RiotCrawler(mongoDbService.Database, httpClientFactory.CreateClient()).Create(configuration.GetRiotApiCrawlerSettings().RiotApiKey);
            _notificationService = notificationService;

            _champions = (_riotApiCrawler.GetChampions(Region.KR).Result).ToDictionary(x => x.Key);
        }

        public async Task ExecuteBackground()
        {
            foreach (var summoner in await _riotApiCrawler.GetTrackingSummoners())
            {
                var playingGame = await _riotApiCrawler.GetCurrentGame(summoner,
                    async (game) =>
                    {
                        var participant = game.Participants.FirstOrDefault(x => x.SummonerName == summoner.Name);

                        var champion = _champions[participant.ChampionId];

                        var message = $"{summoner.Name}님이 {champion.Name}(으)로 {game.GameMode} 모드 게임을 시작하셨습니다";
                        var championImageUrl = $"http://ddragon.leagueoflegends.com/cdn/img/champion/splash/{champion.ChampionId}_0.jpg";
                        await _notificationService.Execute(summoner.Region, message, new List<string> { championImageUrl });
                    });

                if (playingGame != null)
                {
                    var match = await _riotApiCrawler.GetMatch(playingGame.GameId, Region.Get(summoner.Region));
                    if (match != null)
                    {
                        _ = _riotApiCrawler.EndGame(summoner, playingGame);

                        var participantIdentity = match.ParticipantIdentities.FirstOrDefault(x => x.Player.SummonerName == summoner.Name);
                        var participant = match.Participants.FirstOrDefault(x => x.ParticipantId == participantIdentity.ParticipantId);

                        var win = participant.Stats.Win;
                        var k = participant.Stats.Kills;
                        var d = participant.Stats.Deaths;
                        var a = participant.Stats.Assists;
                        var kda = (k + a) / (float)d;

                        var champion = _champions[participant.ChampionId];
                        var championImageUrl = $"http://ddragon.leagueoflegends.com/cdn/img/champion/splash/{champion.ChampionId}_0.jpg";

                        if (win)
                        {
                            var message = $"{summoner.Name}님이 {champion.Name}(으)로 {playingGame.GameMode}모드 게임을 승리하셨습니다. KDA[{kda}, {k}/{d}/{a}]";
                            var winImageUrl = "https://mir-s3-cdn-cf.behance.net/project_modules/1400/c9916f54385211.5959b34077df7.jpg";

                            await _notificationService.Execute(summoner.Region, message, new List<string> { championImageUrl, winImageUrl });
                        }
                        else
                        {
                            var message = $"{summoner.Name}님이 {champion.Name}(으)로 {playingGame.GameMode}모드 게임을 패배하셨습니다. KDA[{kda}, {k}/{d}/{a}]";
                            var loseImageUrl = "https://mir-s3-cdn-cf.behance.net/project_modules/1400/c9ccce54385211.5959b3407819c.jpg";

                            await _notificationService.Execute(summoner.Region, message, new List<string> { championImageUrl, loseImageUrl });
                        }

                        await _riotApiCrawler.RefreshLeagueEntries(summoner.Id, Region.Get(summoner.Region));
                    }
                }
            }
        }
    }
}