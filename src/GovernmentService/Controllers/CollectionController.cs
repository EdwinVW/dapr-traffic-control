using Dapr;
using GovernmentService.Helpers;
using GovernmentService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovernmentService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CollectionController : ControllerBase
    {
        private readonly ILogger<CollectionController> _logger;
        private readonly IFineCalculator _fineCalculator;

        public CollectionController(ILogger<CollectionController> logger, IFineCalculator fineCalculator)
        {
            _logger = logger;
            _fineCalculator = fineCalculator;
        }

        [Route("sendfine")]
        [HttpPost()]
        public ActionResult SendFine(SpeedingViolation speedingViolation)
        {
            decimal fine = _fineCalculator.CalculateFine(speedingViolation.ViolationInKmh);

            string fineString = fine == 0 ? "tbd by the prosecutor" : $"{fine} Euro";
            _logger.LogInformation($"Sent speeding ticket. Road: {speedingViolation.RoadId}, Licensenumber: {speedingViolation.VehicleId}, " +
                $"Violation: {speedingViolation.ViolationInKmh} Km/h, Fine: {fineString}, On: {speedingViolation.Timestamp.ToString("dd-MM-yyyy")} " +
                $"at {speedingViolation.Timestamp.ToString("hh:mm:ss")}.");

            return Ok();
        }
    }
}
