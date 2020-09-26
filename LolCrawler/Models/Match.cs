using MingweiSamuel.Camille.MatchV4;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WebUtil.Models;

namespace LolCrawler.Models
{
    public class Match : MongoDbHeader
    {
        public long GameId { get; set; }

        public ParticipantIdentity[] ParticipantIdentities { get; set; }

        public int QueueId { get; set; }

        public string GameType { get; set; }

        public long GameDuration { get; set; }

        public TeamStats[] Teams { get; set; }

        public string PlatformId { get; set; }

        public long GameCreation { get; set; }

        public int SeasonId { get; set; }

        public string GameVersion { get; set; }

        public int MapId { get; set; }

        public string GameMode { get; set; }

        public Participant[] Participants { get; set; }
    }
}
