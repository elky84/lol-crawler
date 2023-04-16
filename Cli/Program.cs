using LolCrawler.Api;
using LolCrawler.Code;
using MingweiSamuel.Camille.Enums;
using MingweiSamuel.Camille.MatchV5;
using MongoDB.Driver;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cli
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("starting up!");

            var client = new MongoClient("mongodb://localhost:27017/?maxPoolSize=200");
            var database = client.GetDatabase("cli-lol-crawler");

            var riotApiKey = Environment.GetEnvironmentVariable("RIOT_API_KEY");
            if (string.IsNullOrEmpty(riotApiKey))
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(await File.ReadAllTextAsync("config.json"));
                riotApiKey = dict["RiotApiKey"];
            }

            var riot = new RiotCrawler(database, new HttpClient()).Create(riotApiKey);
            const string summonerName = "elky";
            var summoner = await riot.CreateSummerByName(summonerName, Region.KR, true);
            if (null == summoner)
            {
                // If a summoner is not found, the response will be null.
                Log.Logger.Information($"Summoner '{summonerName}' not found.");
                return;
            }

            var currentGame = await riot.GetCurrentGame(summoner);
            if (null != currentGame)
            {
                Log.Logger.Information("<Summoner:{SummonerName}> <currentGameState:{CurrentGameInfo}>", 
                    summonerName, currentGame.Info);
            }

            var matchIds = await riot.RiotApi.MatchV5.GetMatchIdsByPUUIDAsync(MatchRegion.FromRegion(Region.KR), summoner.Puuid);
            // Queue ID 420 is RANKED_SOLO_5v5 (TODO)
            // Queue 참고 https://static.developer.riotgames.com/docs/lol/queues.json
            var matches = matchIds?.Select(async matchId =>
                    await riot.RiotApi.MatchV5.GetMatchAsync(MatchRegion.FromRegion(Region.KR), matchId)
                ).Where(x => x != null)
                .Select(x => x.Result)
                .ToList() ?? new List<Match>();
            
            foreach (var match in matches)
            {
                Log.Logger.Information("Match {InfoGameId}", match.Info.GameId);
            }

            //// 추가로 DB에 저장할 데이터
            //// 1. SummonerChampion (챔피언 숙련도 등)
            //// https://github.com/MingweiSamuel/Camille 가서 챔피언 숙련도 데이터도 가져오기
        }
    }
}