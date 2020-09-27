using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SummonerController : ControllerBase
    {
        private readonly ILogger<SummonerController> _logger;

        private readonly SummonerService _summonerService;

        public SummonerController(ILogger<SummonerController> logger, SummonerService summonerService)
        {
            _logger = logger;
            _summonerService = summonerService;
        }

        [HttpGet]
        public async Task<Protocols.Response.Summoner> Get([FromQuery] Protocols.Request.Summoner summoner)
        {
            return await _summonerService.Get(summoner);
        }

        [HttpPost]
        public async Task<Protocols.Response.Summoner> Create([FromBody] Protocols.Request.Summoner summoner)
        {
            return await _summonerService.Create(summoner);
        }


        [HttpPost("Refresh")]
        public async Task<Protocols.Response.Summoner> Refresh([FromBody] Protocols.Request.Summoner summoner)
        {
            return await _summonerService.Refresh(summoner);
        }


        [HttpPut("{summonerName}")]
        public async Task<Protocols.Response.Summoner> Update(string summonerName, [FromBody] Protocols.Request.Summoner summoner)
        {
            summoner.SummonerName = summonerName;
            return await _summonerService.Update(summoner);
        }

        [HttpDelete("{summonerName}")]
        public async Task<Protocols.Response.Summoner> Delete(string summonerName, [FromBody] Protocols.Request.Summoner summoner)
        {
            summoner.SummonerName = summonerName;
            return await _summonerService.Delete(summoner);
        }
    }
}
