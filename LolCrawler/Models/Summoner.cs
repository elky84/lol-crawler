using MingweiSamuel.Camille.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using WebUtil.Models;

namespace LolCrawler.Models
{
    public class Summoner : MongoDbHeader
    {
        public string Region { get; set; }

        public string SummonerId { get; set; }

        public string AccountId { get; set; }

        public string Name { get; set; }

        public string NameLower { get; set; }

        public string Puuid { get; set; }

        public long Level { get; set; }

        public bool Tracking { get; set; }

        public long? TrackingGameId { get; set; }
    }
}
