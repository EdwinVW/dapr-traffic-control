using Dapr;
using GovernmentService.Events;
using GovernmentService.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovernmentService.Controllers
{
    [ApiController]
    public class CJIBController : ControllerBase
    {
        private readonly ILogger<CJIBController> _logger;
        private readonly IFineCalculator _fineCalculator;

        public CJIBController(ILogger<CJIBController> logger, IFineCalculator fineCalculator)
        {
            _logger = logger;
            _fineCalculator = fineCalculator;
        }

        [Topic("pubsub", "cjib.speedingviolation")]
        [Route("cjib/speedingviolation")]
        [HttpPost()]
        public ActionResult HandleSpeedingViolation(SpeedingViolationDetected @event)
        {
            decimal fine = _fineCalculator.CalculateFine(@event.ViolationInKmh);

            string fineString = fine == 0 ? "tbd by the prosecutor" : $"{fine} Euro";
            _logger.LogInformation($"CJIB: Sent speeding ticket. Road: {@event.RoadId}, Licensenumber: {@event.VehicleId}, " +
                $"Violation: {@event.ViolationInKmh} Km/h, Fine: {fineString}, On: {@event.Timestamp.ToString("dd-MM-yyyy")} " +
                $"at {@event.Timestamp.ToString("hh:mm:ss")}.");

            return Ok();
        }
    }
}
