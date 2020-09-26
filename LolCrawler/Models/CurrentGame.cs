using LolCrawler.Code;
using MingweiSamuel.Camille.SpectatorV4;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WebUtil.Models;

namespace LolCrawler.Models
{
    public class CurrentGame : MongoDbHeader
    {
        [BsonRepresentation(BsonType.String)]
        public GameState GameState { get; set; }

        public long GameId { get; set; }

        public string GameType { get; set; }

        public long GameStartTime { get; set; }

        public long MapId { get; set; }

        public long GameLength { get; set; }

        public string PlatformId { get; set; }

        public string GameMode { get; set; }

        public BannedChampion[] BannedChampions { get; set; }

        public long GameQueueConfigId { get; set; }

        public Observer Observers { get; set; }

        public CurrentGameParticipant[] Participants { get; set; }
    }
}
