using EzMongoDb.Models;
using System.Collections.Generic;

namespace LolCrawler.Models
{
    public class LeagueEntry : MongoDbHeader
    {
        public bool Inactive { get; set; }
        public bool FreshBlood { get; set; }
        public bool Veteran { get; set; }
        public bool HotStreak { get; set; }
        public int Losses { get; set; }
        public int Wins { get; set; }
        public MiniSeries MiniSeries { get; set; }
        public int LeaguePoints { get; set; }
        public string Tier { get; set; }
        public string QueueType { get; set; }
        public string SummonerName { get; set; }
        public string SummonerId { get; set; }
        public string LeagueId { get; set; }
        public string Rank { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public Dictionary<string, object> _AdditionalProperties { get; set; }
    }
}
