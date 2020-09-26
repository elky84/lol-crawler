using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LolCrawler.Api;
using MingweiSamuel.Camille.Enums;
using MongoDB.Driver;
using Newtonsoft.Json;
using Serilog;

namespace Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("starting up!");

            var client = new MongoClient("mongodb://localhost:27017/?maxPoolSize=200");
            var database = client.GetDatabase("cli-lol-crawler");

            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("config.json"));

            var riot = new RiotCrawler(database, new HttpClient()).Create(dict["RiotApiKey"]);
            var summonerName = "sta";
            var summoner = await riot.GetSummerByName(summonerName, Region.KR);
            if (null == summoner)
            {
                // If a summoner is not found, the response will be null.
                Log.Logger.Information($"Summoner '{summonerName}' not found.");
                return;
            }

            var currentGame = await riot.GetCurrentGame(summoner);
            if (null == currentGame)
            {
                // If a summoner is not found, the response will be null.
                Log.Logger.Information($"Summoner '{summonerName}' currentGame not found.");
                return;
            }

            var match = await riot.GetMatch(currentGame.GameId, Region.Get(summoner.Region));

            //// 추가로 DB에 저장할 데이터
            //// 1. SummonerChampion (챔피언 숙련도 등)
            //// https://github.com/MingweiSamuel/Camille 가서 챔피언 숙련도 데이터도 가져오기

            // Get 10 most recent matches (blocking).
            // Queue ID 420 is RANKED_SOLO_5v5 (TODO)
            var matchlist = await riot.RiotApi.MatchV4.GetMatchlistAsync(
               Region.KR, summoner.AccountId, queue: new[] { 420 }, endIndex: 10);
            // Get match results (done asynchronously -> not blocking -> fast).
            var matchDataTasks = matchlist.Matches.Select(
                   matchMetadata => riot.RiotApi.MatchV4.GetMatchAsync(Region.KR, matchMetadata.GameId)
               ).ToArray();
            // Wait for all task requests to complete asynchronously.
            var matchDatas = await Task.WhenAll(matchDataTasks);
        }
    }
}