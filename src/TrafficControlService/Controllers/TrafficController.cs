//#define USE_ACTORMODEL

/* 
This controller contains 2 implementations of the TrafficControl functionality: a basic 
implementation and an actor-model based implementation.

The code for the basic implementation is in this controller. The actor-model implementation 
resides in the Vehicle actor (./Actors/VehicleActor.cs).

To switch between the two implementations, you need to use the USE_ACTORMODEL symbol at
the top of this file. If you comment the #define USE_ACTORMODEL statement, the basic 
implementation is used. Uncomment this statement to use the Actormodel implementation.
*/

namespace TrafficControlService.Controllers;

[ApiController]
[Route("")]
public class TrafficController : ControllerBase
{
    private readonly ILogger<TrafficController> _logger;
    private readonly IVehicleStateRepository _vehicleStateRepository;
    private readonly ISpeedingViolationCalculator _speedingViolationCalculator;
    private readonly string _roadId;

    public TrafficController(
        ILogger<TrafficController> logger,
        IVehicleStateRepository vehicleStateRepository,
        ISpeedingViolationCalculator speedingViolationCalculator)
    {
        _logger = logger;
        _vehicleStateRepository = vehicleStateRepository;
        _speedingViolationCalculator = speedingViolationCalculator;
        _roadId = speedingViolationCalculator.GetRoadId();
    }

#if !USE_ACTORMODEL

    [HttpPost("entrycam")]
    public async Task<ActionResult> VehicleEntryAsync(VehicleRegistered msg)
    {
        try
        {
            // log entry
            _logger.LogInformation($"ENTRY detected in lane {msg.Lane} at {msg.Timestamp.ToString("hh:mm:ss")} " +
                $"of vehicle with license-number {msg.LicenseNumber}.");

            // store vehicle state
            var vehicleState = new VehicleState(msg.LicenseNumber, msg.Timestamp, null);
            await _vehicleStateRepository.SaveVehicleStateAsync(vehicleState);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing ENTRY");
            return StatusCode(500);
        }
    }

    [HttpPost("exitcam")]
    public async Task<ActionResult> VehicleExitAsync(VehicleRegistered msg, [FromServices] DaprClient daprClient)
    {
        try
        {
            // get vehicle state
            var state = await _vehicleStateRepository.GetVehicleStateAsync(msg.LicenseNumber);
            if (state == default(VehicleState))
            {
                return NotFound();
            }

            // log exit
            _logger.LogInformation($"EXIT detected in lane {msg.Lane} at {msg.Timestamp.ToString("hh:mm:ss")} " +
                $"of vehicle with license-number {msg.LicenseNumber}.");

            // update state
            var exitState = state.Value with { ExitTimestamp = msg.Timestamp };
            await _vehicleStateRepository.SaveVehicleStateAsync(exitState);

            // handle possible speeding violation
            int violation = _speedingViolationCalculator.DetermineSpeedingViolationInKmh(exitState.EntryTimestamp, exitState.ExitTimestamp.Value);
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
                await daprClient.PublishEventAsync("pubsub", "speedingviolations", speedingViolation);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing EXIT");
            return StatusCode(500);
        }
    }

#else

        [HttpPost("entrycam")]
        public async Task<ActionResult> VehicleEntryAsync(VehicleRegistered msg)
        {
            try
            {
                var actorId = new ActorId(msg.LicenseNumber);
                var proxy = ActorProxy.Create<IVehicleActor>(actorId, nameof(VehicleActor));
                await proxy.RegisterEntryAsync(msg);
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("exitcam")]
        public async Task<ActionResult> VehicleExitAsync(VehicleRegistered msg)
        {
            try
            {
                var actorId = new ActorId(msg.LicenseNumber);
                var proxy = ActorProxy.Create<IVehicleActor>(actorId, nameof(VehicleActor));
                await proxy.RegisterExitAsync(msg);
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

#endif

}
