using EzAspDotNet.Exception;
using EzAspDotNet.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

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
                catch (Exception e)
                {
                    e.ExceptionLog();
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }


        private void DoWork()
        {
            _trackingService.ExecuteBackground().Wait();
        }
    }
}
