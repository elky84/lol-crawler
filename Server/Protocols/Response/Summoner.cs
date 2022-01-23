
using Server.Protocols.Common;
using System.Collections.Generic;

namespace Server.Protocols.Response
{
    public class Summoner : EzAspDotNet.Protocols.ResponseHeader
    {
        public Common.Summoner Data { get; set; }

        public List<LeagueEntry> LeagueEntries { get; set; }
    }
}
