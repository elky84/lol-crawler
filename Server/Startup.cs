using AutoMapper;
using EzAspDotNet.Exception;
using EzAspDotNet.Services;
using EzAspDotNet.StartUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Server.Services;
using System;
using System.Text;

namespace Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.Console()
                .CreateLogger();

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            EzAspDotNet.Models.MapperUtil.Initialize(
                new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<EzAspDotNet.Notification.Models.Notification, Protocols.Common.Notification>();
                    cfg.CreateMap<Protocols.Common.Notification, EzAspDotNet.Notification.Models.Notification >();

                    cfg.CreateMap<LolCrawler.Models.Summoner, Protocols.Common.Summoner>();
                    cfg.CreateMap<Protocols.Common.Summoner, LolCrawler.Models.Summoner>();

                    cfg.CreateMap<LolCrawler.Models.LeagueEntry, Protocols.Common.LeagueEntry>();
                    cfg.CreateMap<Protocols.Common.LeagueEntry, LolCrawler.Models.LeagueEntry >();

                    cfg.CreateMap<LolCrawler.Models.MiniSeries, Protocols.Common.MiniSeries>();
                    cfg.CreateMap<Protocols.Common.MiniSeries, LolCrawler.Models.MiniSeries >();
                })
            );

            services.CommonConfigureServices();

            services.AddHttpClient();

            services.AddControllers().AddNewtonsoftJson();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
            });

            services.AddSingleton<IHostedService, TrackingLoopingService>();
            services.AddSingleton<IHostedService, WebHookLoopingService>();

            services.AddSingleton<TrackingService>();
            services.AddSingleton<NotificationService>();
            services.AddSingleton<WebHookService>();

            services.AddSingleton<SummonerService>();

            Log.Logger.Information($"Local TimeZone:{TimeZoneInfo.Local}");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "My API V1");
            });
        }
    }
}
