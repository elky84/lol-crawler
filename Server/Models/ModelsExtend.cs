using LolCrawler.Models;
using EzAspDotNet.Models;
using Server.Protocols.Common;

namespace Server.Models
{
    public static class ModelsExtend
    {
        public static T ToProtocol<T>(this T t, MongoDbHeader header)
            where T : Header
        {
            t.Id = header.Id;
            t.Created = header.Created;
            t.Updated = header.Updated;
            return t;
        }

        public static Protocols.Common.Summoner ToProtocol(this LolCrawler.Models.Summoner summoner)
        {
            return new Protocols.Common.Summoner
            {
                Region = summoner.Region,
                SummonerId = summoner.SummonerId,
                AccountId = summoner.AccountId,
                Name = summoner.Name,
                NameLower = summoner.NameLower,
                Puuid = summoner.Puuid,
                Level = summoner.Level,
                Tracking = summoner.Tracking,
                TrackingGameId = summoner.TrackingGameId
            }.ToProtocol(summoner);
        }

        public static Protocols.Common.LeagueEntry ToProtocol(this LolCrawler.Models.LeagueEntry leagueEntry)
        {
            return new Protocols.Common.LeagueEntry
            {
                Inactive = leagueEntry.Inactive,
                MiniSeries = leagueEntry.MiniSeries?.ToProtocol(),
                FreshBlood = leagueEntry.FreshBlood,
                Veteran = leagueEntry.Veteran,
                HotStreak = leagueEntry.HotStreak,
                Losses = leagueEntry.Losses,
                Wins = leagueEntry.Wins,
                LeaguePoints = leagueEntry.LeaguePoints,
                Tier = leagueEntry.Tier,
                QueueType = leagueEntry.QueueType,
                SummonerName = leagueEntry.SummonerName,
                SummonerId = leagueEntry.SummonerId,
                LeagueId = leagueEntry.LeagueId,
                Rank = leagueEntry.Rank,
            }.ToProtocol(leagueEntry);
        }

        public static Protocols.Common.MiniSeries ToProtocol(this LolCrawler.Models.MiniSeries miniSeries)
        {
            return new Protocols.Common.MiniSeries
            {
                Losses = miniSeries.Losses,
                Progress = miniSeries.Progress,
                Target = miniSeries.Target,
                Wins = miniSeries.Wins,
            };
        }

        public static Protocols.Common.Notification ToProtocol(this EzAspDotNet.Notification.Models.Notification notification)
        {
            return new Protocols.Common.Notification
            {
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                CrawlingType = notification.CrawlingType,
            }.ToProtocol(notification);
        }

        public static EzAspDotNet.Notification.Models.Notification ToModel(this Protocols.Common.Notification notification)
        {
            return new EzAspDotNet.Notification.Models.Notification
            {
                Id = notification.Id,
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                CrawlingType = notification.CrawlingType
            };
        }

        public static EzAspDotNet.Notification.Models.Notification ToModel(this Protocols.Common.NotificationCreate notification)
        {
            return new EzAspDotNet.Notification.Models.Notification
            {
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                CrawlingType = notification.CrawlingType,
            };
        }
    }
}
