using EzMongoDb.Models;
using LolCrawler.Code;
using MingweiSamuel.Camille.SpectatorV4;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LolCrawler.Models
{
    public class CurrentGame : MongoDbHeader
    {
        public long GameId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public GameState GameState { get; set; }

        public CurrentGameInfo Info { get; set; }
    }
}
