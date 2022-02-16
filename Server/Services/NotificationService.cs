using EzAspDotNet.Util;
using System.Threading.Tasks;
using EzAspDotNet.Services;
using MongoDB.Driver;
using Server.Models;
using System.Collections.Generic;
using Server.Code;
using EzAspDotNet.Notification.Models;
using EzAspDotNet.Exception;

namespace Server.Services
{
    public class NotificationService
    {
        private readonly MongoDbUtil<Notification> _mongoDbNotification;

        public NotificationService(MongoDbService mongoDbService)
        {
            _mongoDbNotification = new MongoDbUtil<Notification>(mongoDbService.Database);

            _mongoDbNotification.Collection.Indexes.CreateOne(new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending(x => x.CrawlingType)));
        }

        public async Task<List<Notification>> All()
        {
            return await _mongoDbNotification.All();
        }

        public async Task<List<Notification>> Get(FilterDefinition<Notification> filter)
        {
            return await _mongoDbNotification.FindAsync(filter);
        }

        public async Task<Protocols.Response.Notification> Create(Protocols.Request.NotificationCreate notification)
        {
            var created = await Create(notification.Data);

            return new Protocols.Response.Notification
            {
                ResultCode = EzAspDotNet.Code.ResultCode.Success,
                Data = created?.ToProtocol()
            };

        }

        private async Task<Notification> Create(Protocols.Common.NotificationCreate notification)
        {
            try
            {
                return await _mongoDbNotification.UpsertAsync(Builders<Notification>.Filter.Eq(x => x.CrawlingType, notification.CrawlingType), notification.ToModel());
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingNotificationId);
            }
        }

        public async Task<Protocols.Response.NotificationMulti> CreateMulti(Protocols.Request.NotificationMulti notificationMulti)
        {
            var notifications = new List<Notification>();
            foreach (var notification in notificationMulti.Datas)
            {
                notifications.Add(await Create(notification));
            }

            return new Protocols.Response.NotificationMulti
            {
                Datas = notifications.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Notification> Get(string id)
        {
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (await _mongoDbNotification.FindOneAsyncById(id))?.ToProtocol()
            };
        }


        public async Task<Protocols.Response.Notification> Update(string id, Protocols.Request.NotificationUpdate notificationUpdate)
        {
            var update = notificationUpdate.Data.ToModel();

            var updated = await _mongoDbNotification.UpdateAsync(id, update);
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Notification> Delete(string id)
        {
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (await _mongoDbNotification.RemoveGetAsync(id))?.ToProtocol()
            };
        }
    }
}
