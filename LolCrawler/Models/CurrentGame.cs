using LolCrawler.Code;
using MingweiSamuel.Camille.SpectatorV4;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using EzAspDotNet.Models;
using EzMongoDb.Models;

namespace LolCrawler.Models
{
    public class CurrentGame : MongoDbHeader
    {
        [BsonRepresentation(BsonType.String)]
        public GameState GameState { get; set; }

        public CurrentGameInfo Info { get; set; }
    }
}
