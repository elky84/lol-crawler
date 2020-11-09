using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using Server.Models;
using System.Collections.Generic;
using Server.Exception;
using System.Net.Http;
using System.Linq;
using System;
using System.Collections.Concurrent;
using Server.Code;
using System.Threading;
using Serilog;
using LolCrawler.Models;

namespace Server.Services
{
    public class NotificationService
    {
        private readonly MongoDbUtil<Notification> _mongoDbNotification;


        private readonly IHttpClientFactory _httpClientFactory;

        private readonly List<Protocols.Notification.Request.DiscordWebHook> _discordWebHooks =
            new List<Protocols.Notification.Request.DiscordWebHook>();

        public NotificationService(MongoDbService mongoDbService,
            IHttpClientFactory httpClientFactory)
        {
            _mongoDbNotification = new MongoDbUtil<Notification>(mongoDbService.Database);
            _httpClientFactory = httpClientFactory;

            _mongoDbNotification.Collection.Indexes.CreateOne(new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending(x => x.Region)));
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
                ResultCode = ResultCode.Success,
                Data = created?.ToProtocol()
            };

        }

        private async Task<Notification> Create(Protocols.Common.NotificationCreate notification)
        {
            try
            {
                return await _mongoDbNotification.UpsertAsync(Builders<Notification>.Filter.Eq(x => x.Region, notification.Region), notification.ToModel());
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

        public async Task Execute(string region, string content, List<string> imageUrls = null)
        {
            var notification = await _mongoDbNotification.FindOneAsync(Builders<Notification>.Filter.Eq(x => x.Region, region));
            _discordWebHooks.Add(DiscordNotify(notification, content, imageUrls));
        }


        private Protocols.Notification.Request.DiscordWebHook DiscordNotify(Notification notification, string content, List<string> imageUrls)
        {
            return new Protocols.Notification.Request.DiscordWebHook
            {
                username = notification.Name,
                avatar_url = notification.IconUrl,
                content = content,
                HookUrl = notification.HookUrl
            }.AddImage(imageUrls);
        }

        private void ProcessDiscordWebHooks()
        {
            var processList = new ConcurrentBag<Protocols.Notification.Request.DiscordWebHook>();
            Parallel.ForEach(_discordWebHooks.GroupBy(x => x.HookUrl), group =>
            {
                foreach (var webHook in group.Select(x => x))
                {
                    var response = _httpClientFactory.RequestJson(HttpMethod.Post, group.Key, webHook).Result;
                    if (response == null || response.Headers == null)
                    {
                        continue;
                    }

                    var rateLimitRemaining = response.Headers.GetValues("x-ratelimit-remaining").FirstOrDefault().ToInt();
                    var rateLimitAfter = response.Headers.GetValues("x-ratelimit-reset-after").FirstOrDefault().ToInt();
                    if (response.IsSuccessStatusCode)
                    {
                        processList.Add(webHook);
                    }

                    if (rateLimitRemaining <= 1 || rateLimitAfter > 0)
                    {
                        Thread.Sleep((rateLimitAfter + 1) * 1000);
                        continue;
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Log.Logger.Error($"Too Many Requests [{group.Key}] [{rateLimitRemaining}, {rateLimitAfter}]");
                        break;
                    }
                }
            });

            foreach (var process in processList)
            {
                _discordWebHooks.Remove(process);
            }
        }

        public void HttpTaskRun()
        {
            ProcessDiscordWebHooks();
        }
    }
}
