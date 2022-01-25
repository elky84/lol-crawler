﻿using Server.Models;
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
using EzAspDotNet.Services;
using MongoDB.Driver;
using System.Web;
using System.Text;

namespace Server.Services
{
    public class TrackingService
    {
        private readonly RiotCrawler _riotApiCrawler;

        private readonly WebHookService _webHookService;

        private readonly SummonerService _summonerService;

        private Dictionary<long, LolCrawler.Models.Champion> _champions = new Dictionary<long, LolCrawler.Models.Champion>();

        private DateTime OldDate;

        public TrackingService(IConfiguration configuration,
            SummonerService summonerService,
            MongoDbService mongoDbService,
            WebHookService webHookService,
            IHttpClientFactory httpClientFactory)
        {
            _summonerService = summonerService;
            _riotApiCrawler = new RiotCrawler(mongoDbService.Database, httpClientFactory.CreateClient()).Create(configuration.GetRiotApiCrawlerSettings().RiotApiKey);
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

        public async Task ExecuteBackground()
        {
            if (OldDate.Date != DateTime.Now.Date)
                await LoadChampions();

            OldDate = DateTime.Now;

            foreach (var summoner in await _riotApiCrawler.GetTrackingSummoners())
            {
                var builder = Builders<EzAspDotNet.Notification.Models.Notification>.Filter.Empty;
                var summonerTitle = $"소환사 `{summoner.Name}`";

                var playingGame = await _riotApiCrawler.GetCurrentGame(summoner,
                    async (game) =>
                    {
                        var participant = game.Info.Participants.FirstOrDefault(x => x.SummonerId == summoner.SummonerId);
                        if(participant == null)
                        {
                            Log.Error($"Not found Participant. <SummonerName:{summoner.Name}> <SummonerId:{summoner.SummonerId}> <GameId:{game.Info.GameId}>");
                            return;
                        }

                        var summonerDetail = await _summonerService.Refresh(new Protocols.Request.Summoner { SummonerName = summoner.Name,
                            Switch = summoner.Tracking, Region = summoner.Region});

                        var champion = _champions[participant.ChampionId];
                        var championImageUrl = $"http://ddragon.leagueoflegends.com/cdn/img/champion/splash/{champion.ChampionId}_0.jpg";

                        var message = $"`게임 시작` (소환사|Lv.{summoner.Level} `{summoner.Name}`)\n (챔피언|{champion.Name})\n (게임모드|{game.Info.GameType}/{game.Info.GameMode})\n";

                        summonerDetail.LeagueEntries.ForEach(x => message += x.ToString());

                        await _webHookService.Execute(builder,
                            summonerTitle,
                            message,
                            summoner.Name,
                            $"https://www.op.gg/summoner/userName={HttpUtility.UrlEncode(summoner.Name, Encoding.UTF8)}",
                            DateTime.Now,
                            new List<string> { championImageUrl });
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
                                return;
                            }

                            var participant = match.Info.Participants.FirstOrDefault(x => x.ParticipantId == participantIdentity.ParticipantId);
                            if (participant == null)
                            {
                                Log.Error($"Not found participant. <SummonerName:{summoner.Name}> <SummonerId:{summoner.SummonerId}> <GameId:{match.Info.GameId}>");
                                return;
                            }

                            var win = participant.Win;
                            var k = participant.Kills;
                            var d = participant.Deaths;
                            var a = participant.Assists;
                            var kda = (k + a) / (float)d;

                            var summonerDetail = await _summonerService.Refresh(new Protocols.Request.Summoner
                            {
                                SummonerName = summoner.Name,
                                Switch = summoner.Tracking,
                                Region = summoner.Region
                            });

                            var champion = _champions[participant.ChampionId];
                            var championImageUrl = $"http://ddragon.leagueoflegends.com/cdn/img/champion/splash/{champion.ChampionId}_0.jpg";

                            var message = $"`{(win ? "승리" : "패배")}` (소환사|Lv.{summoner.Level} `{summoner.Name}`)\n (챔피언|{champion.Name})\n (게임모드|{playingGame.Info.GameType}/{playingGame.Info.GameMode})\n";

                            summonerDetail.LeagueEntries.ForEach(x => message += x.ToString());

                            var resultImageUrl = win ? 
                                                "https://mir-s3-cdn-cf.behance.net/project_modules/1400/c9916f54385211.5959b34077df7.jpg" :
                                                "https://mir-s3-cdn-cf.behance.net/project_modules/1400/c9ccce54385211.5959b3407819c.jpg";

                            await _webHookService.Execute(builder,
                                summonerTitle,
                                message,
                                summoner.Name,
                                $"https://www.op.gg/summoner/userName={HttpUtility.UrlEncode(summoner.Name, Encoding.UTF8)}",
                                DateTime.Now,
                                new List<string> { championImageUrl, resultImageUrl });
                        });
                }
            }
        }
    }
}