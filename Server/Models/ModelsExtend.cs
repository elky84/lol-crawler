using LolCrawler.Models;
using WebUtil.Models;

namespace Server.Models
{
    public static class ModelsExtend
    {
        public static T ToProtocol<T>(this T t, MongoDbHeader header)
            where T : Protocols.Common.Header
        {
            t.Id = header.Id;
            t.Created = header.Created;
            return t;
        }

        public static Protocols.Common.Summoner ToProtocol(this Summoner summoner)
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

        public static Protocols.Common.Notification ToProtocol(this Notification notification)
        {
            return new Protocols.Common.Notification
            {
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                Region = notification.Region,
            }.ToProtocol(notification);
        }

        public static Notification ToModel(this Protocols.Common.Notification notification)
        {
            return new Notification
            {
                Id = notification.Id,
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                Region = notification.Region
            };
        }

        public static Notification ToModel(this Protocols.Common.NotificationCreate notification)
        {
            return new Notification
            {
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                Region = notification.Region,
            };
        }
    }
}
