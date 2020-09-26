using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Common
{
    public class Summoner : Header
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
