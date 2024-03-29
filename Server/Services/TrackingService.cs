﻿using EzAspDotNet.Services;
using EzAspDotNet.Util;
using LolCrawler.Api;
using LolCrawler.Models;
using Microsoft.Extensions.Configuration;
using MingweiSamuel.Camille.Enums;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Server.Services
{
    public class TrackingService
    {
        private readonly RiotCrawler _riotApiCrawler;

        private readonly WebHookService _webHookService;

        private readonly SummonerService _summonerService;

        private Dictionary<long, LolCrawler.Models.Champion> _champions = new Dictionary<long, LolCrawler.Models.Champion>();

        private DateTime _oldDate;

        public TrackingService(IConfiguration configuration,
            SummonerService summonerService,
            MongoDbService mongoDbService,
            WebHookService webHookService,
            IHttpClientFactory httpClientFactory)
        {
            _summonerService = summonerService;
            _riotApiCrawler = new RiotCrawler(mongoDbService.Database, httpClientFactory.CreateClient()).Create(Environment.GetEnvironmentVariable("RIOT_API_KEY"));
            _webHookService = webHookService;
        }

        private async Task LoadChampions()
        {
            var champions = await _riotApiCrawler.GetChampions(Region.KR);
            if (champions != null)
            {
                _champions = champions.ToDictionary(x => x.Key);
            }
        }

        private async Task SendWebHook(Summoner summoner,
            LolCrawler.Models.Champion champion,
            CurrentGame game,
            MingweiSamuel.Camille.MatchV5.Participant participant)
        {
            var summonerDetail = await _summonerService.Refresh(summoner);

            var championImageUrl = $"http://ddragon.leagueoflegends.com/cdn/img/champion/splash/{champion.ChampionId}_0.jpg";
            var webHook = new EzAspDotNet.Notification.Data.WebHook
            {
                Author = summoner.Name,
                AuthorLink = $"https://www.op.gg/summoner/userName={HttpUtility.UrlEncode(summoner.Name, Encoding.UTF8)}",
                TimeStamp = DateTime.UtcNow.ToTimeStamp(),
                ImageUrl = championImageUrl,
                AuthorIcon = "https://opgg-com-image.akamaized.net/attach/images/20190416173507.228538.png",
                Footer = $"Lv.{summoner.Level} `{summoner.Name}`",
                FooterIcon = "https://opgg-com-image.akamaized.net/attach/images/20190416173507.228538.png",
            };

            var webHooks = new List<EzAspDotNet.Notification.Data.WebHook> { webHook };

            if (participant == null)
            {
                webHook.Title = $"게임 시작 `{summoner.Name}`";
            }
            else
            {
                var win = participant.Win;
                var k = participant.Kills;
                var d = participant.Deaths;
                var a = participant.Assists;
                var kda = (k + a) / (float)d;

                webHooks.Add(new EzAspDotNet.Notification.Data.WebHook
                {
                    ImageUrl = win ?
                               "https://mir-s3-cdn-cf.behance.net/project_modules/1400/c9916f54385211.5959b34077df7.jpg" :
                               "https://mir-s3-cdn-cf.behance.net/project_modules/1400/c9ccce54385211.5959b3407819c.jpg"
                });

                webHook.Title = $"게임 {(win ? "승리" : "패배")} `{summoner.Name}`";

                webHook.Fields.Add(new EzAspDotNet.Notification.Data.Field
                {
                    Title = "KDA",
                    Value = $"{k}/{d}/{a}, KDA:{kda}"
                });
            }

            webHook.Fields.Add(new EzAspDotNet.Notification.Data.Field
            {
                Title = "챔피언",
                Value = champion.Name
            });

            webHook.Fields.Add(new EzAspDotNet.Notification.Data.Field
            {
                Title = "게임모드",
                Value = $"{game.Info.GameType}/{game.Info.GameMode}"
            });

            foreach (var leagueEntry in summonerDetail.LeagueEntries.Where(x => !string.IsNullOrEmpty(x.Rank) || !string.IsNullOrEmpty(x.Tier)))
            {
                webHook.Fields.Add(new EzAspDotNet.Notification.Data.Field
                {
                    Title = $"리그 <{leagueEntry.QueueType}>",
                    Value = $"`{leagueEntry.Tier} {leagueEntry.Rank}` {leagueEntry.LeaguePoints} Lp"
                });

                webHook.Fields.Add(new EzAspDotNet.Notification.Data.Field
                {
                    Title = $"승{leagueEntry.Wins} 패{leagueEntry.Losses} - 승률",
                    Value = $"{leagueEntry.Wins / (leagueEntry.Wins + (double)leagueEntry.Losses) * 100.0}%"
                });
            }

            await _webHookService.Execute(Builders<EzAspDotNet.Notification.Models.Notification>.Filter.Empty, webHooks);
        }

        public async Task ExecuteBackground()
        {
            if (_oldDate.Date != DateTime.Now.Date)
                await LoadChampions();

            _oldDate = DateTime.Now;


            var trackingSummoners = await _riotApiCrawler.GetTrackingSummoners();

            foreach (var g in trackingSummoners.GroupBy(x => x.TrackingGameId ?? trackingSummoners.IndexOf(x)))
            {
                var repSummoner = g.First();
                var playingGame = await _riotApiCrawler.GetCurrentGame(repSummoner,
                async (game) =>
                {
                    foreach (var summoner in g.AsParallel())
                    {
                        var participant = game.Info.Participants.FirstOrDefault(x => x.SummonerId == repSummoner.SummonerId);
                        if (participant == null)
                        {
                            Log.Error("Not found Participant. <SummonerName:{RepSummonerName}> <SummonerId:{RepSummonerSummonerId}> <GameId:{GameGameId}>", repSummoner.Name, repSummoner.SummonerId, game.GameId);
                            return;
                        }

                        var champion = _champions[participant.ChampionId];
                        await SendWebHook(repSummoner, champion, game, null);
                    }
                });

                if (playingGame is { GameState: LolCrawler.Code.GameState.Playing })
                {
                    await _riotApiCrawler.GetMatch(repSummoner, playingGame.GameId, Region.Get(repSummoner.Region),
                        async (match) =>
                        {
                            foreach (var summoner in g.AsParallel())
                            {
                                _riotApiCrawler.EndGame(summoner, playingGame);

                                var participantIdentity = match.Info.Participants.FirstOrDefault(x => x.SummonerId == summoner.SummonerId);
                                if (participantIdentity == null)
                                {
                                    Log.Error("Not found participantIdentity. <SummonerName:{SummonerName}> <SummonerId:{SummonerSummonerId}> <GameId:{MatchGameId}>", summoner.Name, summoner.SummonerId, match.GameId);
                                    return;
                                }

                                var participant = match.Info.Participants.FirstOrDefault(x => x.ParticipantId == participantIdentity.ParticipantId);
                                if (participant == null)
                                {
                                    Log.Error(
                                        "Not found participant. <SummonerName:{SummonerName}> <SummonerId:{SummonerSummonerId}> <GameId:{MatchGameId}>",
                                        summoner.Name, summoner.SummonerId, match.GameId);
                                    return;
                                }

                                var champion = _champions[participant.ChampionId];

                                await SendWebHook(summoner, champion, playingGame, participant);
                            }
                        });
                }
            }
        }
    }
}