using EzMongoDb.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

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
        
        public long Key { get; set; }

        public string Lore { get; set; }

        [JsonProperty("Id")]
        public string ChampionId { get; set; }

        public string Blurb { get; set; }
    }
}
