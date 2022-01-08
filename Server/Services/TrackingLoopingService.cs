﻿using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using System.Threading;
using System;
using EzAspDotNet.Exception;

namespace Server.Services
{
    public class TrackingLoopingService : LoopingService
    {
        private readonly TrackingService _trackingService;

        public TrackingLoopingService(TrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DoWork();
                }
                catch (System.Exception e)
                {
                    e.ExceptionLog();
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }


        protected void DoWork()
        {
            _trackingService.ExecuteBackground().Wait();
        }
    }
}
