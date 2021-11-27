using MingweiSamuel.Camille.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LolCrawler.Code
{
    public static class MatchRegion
    {
        public static Region FromRegion(Region region)
        {
            if (region.Key == Region.NA.Key ||
                region.Key == Region.BR.Key ||
                region.Key == Region.LAN.Key ||
                region.Key == Region.LAS.Key ||
                region.Key == Region.OCE.Key)
            {
                return Region.Americas;
            }
            else if (region.Key == Region.KR.Key ||
                region.Key == Region.JP.Key)
            {
                return Region.Asia;
            }
            else if (region.Key == Region.EUNE.Key ||
                region.Key == Region.EUW.Key ||
                region.Key == Region.TR.Key ||
                region.Key == Region.RU.Key)
            {
                return Region.Europe;
            }
            else
            {
                throw new Exception($"Invalid Region To MatchRegion {region.Key}");
            }
        }
    }
}
