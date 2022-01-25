using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class LeagueEntry : Header
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

        public override string ToString()
        {
            return $"(리그 {QueueType}|{(string.IsNullOrEmpty(Tier) ? Rank : Tier + "/" + Rank)} {LeaguePoints}, WinLose {Wins}/{Losses} WinRate {(double)Wins / ((double)Wins + (double)Losses) * 100.0}%";
        }
    }
}
