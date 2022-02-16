using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace EzAspDotNet.Settings
{
    public static class Extend
    {
        public static RiotApiCrawlerSettings GetRiotApiCrawlerSettings(this IConfiguration configuration)
        {
            var databaseSection = configuration.GetSection("RiotApiCrawler");
            return databaseSection.Get<RiotApiCrawlerSettings>();
        }
    }
}
