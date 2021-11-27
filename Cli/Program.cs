using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LolCrawler.Api;
using LolCrawler.Code;
using MingweiSamuel.Camille.Enums;
using MingweiSamuel.Camille.MatchV5;
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
            var summonerName = "elky";
            var summoner = await riot.CreateSummerByName(summonerName, Region.KR, true);
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

            var matchIds = await riot.RiotApi.MatchV5.GetMatchIdsByPUUIDAsync(MatchRegion.FromRegion(Region.KR), summoner.Puuid);
            // Queue ID 420 is RANKED_SOLO_5v5 (TODO)
            // Queue 참고 https://static.developer.riotgames.com/docs/lol/queues.json
            List<Match> matches = new();
            foreach (var matchId in matchIds)
            {
                // 현재 게임에 대한 matchMetadata가 있으면 디스코드 알림하고, 현재 게임 정보 상태를 바꾸고, 폴링 대상에서 제거
                var matchData = await riot.RiotApi.MatchV5.GetMatchAsync(MatchRegion.FromRegion(Region.KR), matchId);
                if (matchData == null)
                {
                    continue;
                }

                matches.Add(matchData);
            }

            //// 추가로 DB에 저장할 데이터
            //// 1. SummonerChampion (챔피언 숙련도 등)
            //// https://github.com/MingweiSamuel/Camille 가서 챔피언 숙련도 데이터도 가져오기
        }
    }
}