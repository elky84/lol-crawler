using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Protocols.Common
{
    public class MiniSeries
    {
        public int Losses { get; set; }
        public string Progress { get; set; }
        public int Target { get; set; }
        public int Wins { get; set; }
    }
}
