using EzMongoDb.Models;
using MingweiSamuel.Camille.MatchV5;
using System.Text.Json.Serialization;

namespace LolCrawler.Models
{
    public class Match : MongoDbHeader
    {
        public long GameId { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata Metadata
        {
            get;
            set;
        }

        [JsonPropertyName("info")]
        public Info Info
        {
            get;
            set;
        }

    }
}
