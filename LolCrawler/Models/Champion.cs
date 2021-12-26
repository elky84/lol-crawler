using MingweiSamuel.Camille.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using MongoDbWebUtil.Models;

namespace LolCrawler.Models
{
    public class Champion : MongoDbHeader
    {

        public ChampionInfo Info { get; set; }

        public ChampionStats Stats { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public ChampionImage Image { get; set; }

        public List<string> Tags { get; set; }

        public string Partype { get; set; }

        public long Key { get; set; }

        public string Lore { get; set; }

        [JsonProperty("Id")]
        public string ChampionId { get; set; }

        public string Blurb { get; set; }
    }
}
