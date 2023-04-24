using System;
using System.Collections.Generic;
using System.Text;

namespace LolCrawler.Models
{
    public class MiniSeries
    {
        public int Losses { get; set; }
        public string Progress { get; set; }
        public int Target { get; set; }
        public int Wins { get; set; }

        // ReSharper disable once InconsistentNaming
        public Dictionary<string, object> _AdditionalProperties { get; set; }
    }
}
