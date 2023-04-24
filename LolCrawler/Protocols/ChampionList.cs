using LolCrawler.Models;
using System.Collections.Generic;

namespace LolCrawler.Protocols
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ChampionList
    {
        public Dictionary<string, string> Keys;
        
        // ReSharper disable once UnassignedField.Global
        public Dictionary<string, Champion> Data;
        
        public string Version;
        
        public string Type;
        
        public string Format;
    }
}
