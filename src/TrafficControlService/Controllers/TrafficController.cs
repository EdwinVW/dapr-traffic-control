using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrafficControlService.Events;
using TrafficControlService.DomainServices;
using TrafficControlService.Models;

namespace TrafficControlService.Controllers
{
    [ApiController]
    [Route("")]
    public class TrafficController : ControllerBase
    {
        private const string DAPR_STORE_NAME = "statestore";
        private readonly ILogger<TrafficController> _logger;
        private readonly ISpeedingViolationCalculator _speedingViolationCalculator;
        private readonly string _roadId;

        public TrafficController(
            ILogger<TrafficController> logger, 
            ISpeedingViolationCalculator speedingViolationCalculator)
        {
            _logger = logger;
            _speedingViolationCalculator = speedingViolationCalculator;
            _roadId = speedingViolationCalculator.GetRoadId();
        }

        [HttpPost("entrycam")]
        public async Task<ActionResult> VehicleEntry(
            VehicleRegistered msg, 
            [FromServices] DaprClient daprClient)
        {
            try
            {
                // log entry
                _logger.LogInformation($"ENTRY detected in lane {msg.Lane} at {msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}.");

                // store vehicle state
                var vehicleState = new VehicleState
                {
                    LicenseNumber = msg.LicenseNumber,
                    EntryTimestamp = msg.Timestamp
                };
                await daprClient.SaveStateAsync<VehicleState>(DAPR_STORE_NAME, msg.LicenseNumber, vehicleState);

                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("exitcam")]
        public async Task<ActionResult> VehicleExit(
            VehicleRegistered msg,
            [FromServices] DaprClient daprClient)
        {
            try
            {
                // get vehicle state
                var state = await daprClient.GetStateEntryAsync<VehicleState>(DAPR_STORE_NAME, msg.LicenseNumber);
                if (state.Value == null)
                {
                    return NotFound();
                }

                // log exit
                _logger.LogInformation($"EXIT detected in lane {msg.Lane} at {msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}.");

                // update state
                state.Value.ExitTimestamp = msg.Timestamp;
                await state.SaveAsync();

                // handle possible speeding violation
                int violation = _speedingViolationCalculator.DetermineSpeedingViolationInKmh(state.Value.EntryTimestamp, state.Value.ExitTimestamp);
                if (violation > 0)
                {
                    _logger.LogInformation($"Speeding violation detected ({violation} KMh) of vehicle" +
                        $"with license-number {state.Value.LicenseNumber}.");

                    var speedingViolation = new SpeedingViolation
                    {
                        VehicleId = msg.LicenseNumber,
                        RoadId = _roadId,
                        ViolationInKmh = violation,
                        Timestamp = msg.Timestamp
                    };

                    // publish speedingviolation (Dapr publish / subscribe)
                    await daprClient.PublishEventAsync("pubsub", "collectfine", speedingViolation);
                }

                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}
