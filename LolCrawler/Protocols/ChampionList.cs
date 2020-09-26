using LolCrawler.Models;
using System.Collections.Generic;

namespace LolCrawler.Protocols
{
    public class ChampionList
    {
        public Dictionary<string, string> Keys;
        public Dictionary<string, Champion> Data;
        public string Version;
        public string Type;
        public string Format;
    }
}
