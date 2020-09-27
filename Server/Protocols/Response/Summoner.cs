
using System.Collections.Generic;

namespace Server.Protocols.Response
{
    public class Summoner : Header
    {
        public Common.Summoner SummonerData { get; set; }

        public List<Common.LeagueEntry> LeagueEntries { get; set; }
    }
}
