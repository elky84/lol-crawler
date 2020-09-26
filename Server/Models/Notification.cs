using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq;
using WebUtil.Models;

namespace Server.Models
{
    public class Notification : MongoDbHeader
    {
        public string Name { get; set; }

        public string HookUrl { get; set; }

        public string Channel { get; set; }

        public string IconUrl { get; set; }

        public string Region { get; set; }
    }
}
