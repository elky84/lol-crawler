﻿using EzAspDotNet.Models;
using EzAspDotNet.Notification.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;

        private readonly NotificationService _notificationService;

        public NotificationController(ILogger<NotificationController> logger, NotificationService sourceService)
        {
            _logger = logger;
            _notificationService = sourceService;
        }


        [HttpGet]
        public async Task<Protocols.Response.NotificationMulti> All()
        {
            return new Protocols.Response.NotificationMulti
            {
                Datas = MapperUtil.Map<List<Notification>, List<Protocols.Common.Notification>>(await _notificationService.All())
            };
        }

        [HttpPost]
        public async Task<Protocols.Response.Notification> Create([FromBody] Protocols.Request.NotificationCreate notification)
        {
            return await _notificationService.Create(notification);
        }

        [HttpPost("Multi")]
        public async Task<Protocols.Response.NotificationMulti> CreateMulti([FromBody] Protocols.Request.NotificationMulti notificationMulti)
        {
            return await _notificationService.CreateMulti(notificationMulti);
        }


        [HttpGet("{id}")]
        public async Task<Protocols.Response.Notification> Get(string id)
        {
            return await _notificationService.Get(id);
        }

        [HttpPut("{id}")]
        public async Task<Protocols.Response.Notification> Update(string id, [FromBody] Protocols.Request.NotificationUpdate notification)
        {
            return await _notificationService.Update(id, notification);
        }

        [HttpDelete("{id}")]
        public async Task<Protocols.Response.Notification> Delete(string id)
        {
            return await _notificationService.Delete(id);
        }
    }
}
