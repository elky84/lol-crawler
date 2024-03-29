﻿using EzAspDotNet.Exception;
using EzAspDotNet.Models;
using EzAspDotNet.Services;
using LolCrawler.Api;
using LolCrawler.Models;
using Microsoft.Extensions.Configuration;
using MingweiSamuel.Camille.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Server.Services
{
    public class SummonerService
    {
        private readonly RiotCrawler _riotApiCrawler;

        public SummonerService(IConfiguration configuration,
            MongoDbService mongoDbService,
            IHttpClientFactory httpClientFactory)
        {
            _riotApiCrawler = new RiotCrawler(mongoDbService.Database, httpClientFactory.CreateClient()).Create(Environment.GetEnvironmentVariable("RIOT_API_KEY"));
        }

        public async Task<Protocols.Response.Summoner> Get(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.GetSummerByName(summoner.SummonerName, Region.Get(summoner.Region));
            if (data == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundSummoner);
            }

            var leagueEntries = await _riotApiCrawler.GetLeagueEntries(data);
            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Summoner>(data),
                LeagueEntries = MapperUtil.Map<List<LolCrawler.Models.LeagueEntry>,
                                               List<Protocols.Common.LeagueEntry>>
                                               (leagueEntries)
            };
        }

        public async Task<Protocols.Response.Summoner> Create(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.CreateSummerByName(summoner.SummonerName, Region.Get(summoner.Region), summoner.Switch.GetValueOrDefault(false));
            if (data == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundSummoner);
            }

            var leagueEntries = await _riotApiCrawler.GetLeagueEntries(data);
            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Summoner>(data),
                LeagueEntries = MapperUtil.Map<List<LolCrawler.Models.LeagueEntry>,
                               List<Protocols.Common.LeagueEntry>>
                               (leagueEntries)
            };
        }

        public async Task<Protocols.Response.Summoner> Refresh(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.RefreshSummoner(summoner.SummonerName, Region.Get(summoner.Region), summoner.Switch.GetValueOrDefault(false));
            if (data == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundSummoner);
            }

            return await RefreshAndResponse(data);
        }

        public async Task<Protocols.Response.Summoner> Refresh(Summoner summoner)
        {
            var data = await _riotApiCrawler.RefreshSummonerById(summoner.SummonerId, Region.Get(summoner.Region), summoner.Tracking);
            if (data == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundSummoner);
            }

            return await RefreshAndResponse(data);
        }

        private async Task<Protocols.Response.Summoner> RefreshAndResponse(Summoner summoner)
        {
            var leagueEntries = await _riotApiCrawler.RefreshLeagueEntries(summoner);
            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Summoner>(summoner),
                LeagueEntries = MapperUtil.Map<List<LeagueEntry>,
                               List<Protocols.Common.LeagueEntry>>
                               (leagueEntries)
            };
        }

        public async Task<Protocols.Response.Summoner> Update(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.UpdateSummerByName(summoner.SummonerName, Region.Get(summoner.Region), summoner.Switch.GetValueOrDefault(false));
            if (data == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundSummoner);
            }

            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Summoner>(data),
            };
        }

        public async Task<Protocols.Response.Summoner> Delete(Protocols.Request.Summoner summoner)
        {
            var data = await _riotApiCrawler.DeleteSummerByName(summoner.SummonerName, Region.Get(summoner.Region));
            if (data == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundSummoner);
            }

            await _riotApiCrawler.DeleteLeagueEntries(data);
            return new Protocols.Response.Summoner
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Summoner>(data),
            };
        }
    }
}