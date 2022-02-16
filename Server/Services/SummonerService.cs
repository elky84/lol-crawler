using Server.Models;
using System.Threading.Tasks;
using EzAspDotNet.Services;
using LolCrawler.Api;
using MingweiSamuel.Camille.Enums;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using EzAspDotNet.Settings;

namespace Server.Services
{
    public class SummonerService
    {
        private readonly RiotCrawler _riotApiCrawler;

        public SummonerService(IConfiguration configuration,
            MongoDbService mongoDbService,
            IHttpClientFactory httpClientFactory)
        {
            _riotApiCrawler = new RiotCrawler(mongoDbService.Database, httpClientFactory.CreateClient()).Create(configuration.GetRiotApiCrawlerSettings().RiotApiKey);
        }

        public async Task<Protocols.Response.Summoner> Get(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.GetSummerByName(summoner.SummonerName, Region.Get(summoner.Region));
            var leagueEntries = await _riotApiCrawler.GetLeagueEntries(data);
            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Code.ResultCode.Success,
                Data = data?.ToProtocol(),
                LeagueEntries = leagueEntries.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Summoner> Create(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.CreateSummerByName(summoner.SummonerName, Region.Get(summoner.Region), summoner.Switch.GetValueOrDefault(false));
            var leagueEntries = await _riotApiCrawler.GetLeagueEntries(data);
            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Code.ResultCode.Success,
                Data = data?.ToProtocol(),
                LeagueEntries = leagueEntries.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Summoner> Refresh(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.RefreshSummoner(summoner.SummonerName, Region.Get(summoner.Region), summoner.Switch.GetValueOrDefault(false));
            var leagueEntries = await _riotApiCrawler.RefreshLeagueEntries(data);
            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Code.ResultCode.Success,
                Data = data?.ToProtocol(),
                LeagueEntries = leagueEntries.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Summoner> Update(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.UpdateSummerByName(summoner.SummonerName, Region.Get(summoner.Region), summoner.Switch.GetValueOrDefault(false));
            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Code.ResultCode.Success,
                Data = data?.ToProtocol()
            };
        }

        public async Task<Protocols.Response.Summoner> Delete(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.DeleteSummerByName(summoner.SummonerName, Region.Get(summoner.Region));
            await _riotApiCrawler.DeleteLeagueEntries(data);
            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Code.ResultCode.Success,
                Data = data?.ToProtocol()
            };
        }
    }
}