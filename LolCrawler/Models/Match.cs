using MingweiSamuel.Camille.MatchV5;
using System.Text.Json.Serialization;
using WebUtil.Models;

namespace LolCrawler.Models
{
    public class Match : MongoDbHeader
    {
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
