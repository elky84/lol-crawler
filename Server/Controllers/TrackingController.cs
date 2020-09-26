using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrackingController : ControllerBase
    {
        private readonly ILogger<TrackingController> _logger;

        private readonly TrackingService _trackingService;

        public TrackingController(ILogger<TrackingController> logger, TrackingService trackingService)
        {
            _logger = logger;
            _trackingService = trackingService;
        }

        [HttpGet]
        public async Task<Protocols.Response.Tracking> Get([FromQuery] Protocols.Request.Tracking tracking)
        {
            return await _trackingService.Get(tracking);
        }

        [HttpPost]
        public async Task<Protocols.Response.Tracking> Create([FromBody] Protocols.Request.Tracking tracking)
        {
            return await _trackingService.Create(tracking);
        }


        [HttpPut("{summonerName}")]
        public async Task<Protocols.Response.Tracking> Update([FromBody] Protocols.Request.Tracking tracking)
        {
            return await _trackingService.Update(tracking);
        }

        [HttpDelete("{summonerName}")]
        public async Task<Protocols.Response.Tracking> Delete([FromBody] Protocols.Request.Tracking tracking)
        {
            return await _trackingService.Delete(tracking);
        }
    }
}
