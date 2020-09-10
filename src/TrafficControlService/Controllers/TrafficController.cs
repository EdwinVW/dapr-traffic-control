using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrafficControlService.Events;
using TrafficControlService.Helpers;
using TrafficControlService.Models;

namespace TrafficControlService.Controllers
{
    [ApiController]
    [Route("trafficcontrol")]
    public class TrafficController : ControllerBase
    {
        private const string DAPR_STORE_NAME = "statestore";
        private readonly ILogger<TrafficController> _logger;
        private readonly ISpeedingViolationCalculator _speedingViolationCalculator;
        private readonly string _roadId;

        public TrafficController(ILogger<TrafficController> logger, ISpeedingViolationCalculator speedingViolationCalculator)
        {
            _logger = logger;
            _speedingViolationCalculator = speedingViolationCalculator;
            _roadId = speedingViolationCalculator.GetRoadId();
        }

        [Topic("pubsub-nats", "trafficcontrol.entrycam")]
        [HttpPost("entrycam")]
        public async Task<ActionResult> VehicleEntry(VehicleRegistered msg, [FromServices] DaprClient daprClient)
        {
            // get vehicle details
            // TODO get from RDW
            var vehicleInfo = new VehicleInfo
            {
                VehicleId = msg.LicenseNumber,
                Brand = "Volkswagen",
                Model = "Tiguan"
            };

            // log entry
            _logger.LogInformation($"ENTRY detected of {vehicleInfo.Brand} {vehicleInfo.Model} with license-number " +
                $"{msg.LicenseNumber} at {msg.Timestamp.ToString("hh:mm:ss")}");

            // store vehicle state
            var vehicleState = new VehicleState
            {
                LicenseNumber = msg.LicenseNumber,
                Brand = vehicleInfo.Brand,
                Model = vehicleInfo.Model,
                EntryTimestamp = msg.Timestamp
            };
            await daprClient.SaveStateAsync<VehicleState>(DAPR_STORE_NAME, msg.LicenseNumber, vehicleState);

            return Ok();
        }

        [Topic("pubsub-nats", "trafficcontrol.exitcam")]
        [HttpPost("exitcam")]
        public async Task<ActionResult> VehicleExit(VehicleRegistered msg, [FromServices] DaprClient daprClient)
        {
            // get vehicle state
            var state = await daprClient.GetStateEntryAsync<VehicleState>(DAPR_STORE_NAME, msg.LicenseNumber);
            if (state.Value == null)
            {
                return NotFound();
            }

            // log exit
            _logger.LogInformation($"EXIT detected of {state.Value.Brand} {state.Value.Model} with license-number " +
                $"{msg.LicenseNumber} at {msg.Timestamp.ToString("hh:mm:ss")}");

            // update state
            state.Value.ExitTimestamp = msg.Timestamp;
            await state.SaveAsync();

            // handle possible speeding violation
            int violation = _speedingViolationCalculator.DetermineSpeedingViolationInKmh(state.Value.EntryTimestamp, state.Value.ExitTimestamp);
            if (violation > 0)
            {
                _logger.LogWarning($"Speeding violation detected ({violation} KMh) of {state.Value.Brand} {state.Value.Model} " +
                    $"with license-number {state.Value.LicenseNumber}.");

                var @event = new SpeedingViolationDetected
                {
                    VehicleId = msg.LicenseNumber,
                    RoadId = _roadId,
                    ViolationInKmh = violation
                };
                await daprClient.PublishEventAsync<SpeedingViolationDetected>("pubsub-nats", "cjib.speedingviolation", @event);
            }

            return Ok();
        }
    }
}
