namespace TrafficControlService.Controllers;

[ApiController]
[Route("")]
public class TrafficController : ControllerBase
{
    private const string DAPR_STORE_NAME = "statestore";
    private readonly ILogger<TrafficController> _logger;
    private readonly ISpeedingViolationCalculator _speedingViolationCalculator;
    private readonly DaprClient _daprClient;
    private readonly string _roadId;

    public TrafficController(
        ILogger<TrafficController> logger,
        ISpeedingViolationCalculator speedingViolationCalculator,
        DaprClient daprClient)
    {
        _logger = logger;
        _speedingViolationCalculator = speedingViolationCalculator;
        _daprClient = daprClient;
        _roadId = speedingViolationCalculator.GetRoadId();
    }

    [HttpPost("entrycam")]
    public async Task<ActionResult> VehicleEntryAsync(VehicleRegistered msg)
    {
        try
        {
            // log entry
            _logger.LogInformation("Entry detected in lane {Lane} at {Timestamp} of vehicle with license-number {LicenseNumber}.", msg.Lane, msg.Timestamp, msg.LicenseNumber);

            // store vehicle state
            var vehicleState = new VehicleState(msg.LicenseNumber, msg.Timestamp);
            await _daprClient.SaveStateAsync<VehicleState>(DAPR_STORE_NAME, vehicleState.LicenseNumber, vehicleState);

            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while processing entry");
            throw;
        }
    }

    [HttpPost("exitcam")]
    public async Task<ActionResult> VehicleExitAsync(VehicleRegistered msg)
    {
        try
        {
            // get vehicle state
            var state = await _daprClient.GetStateEntryAsync<VehicleState>(DAPR_STORE_NAME, msg.LicenseNumber);
            if (state.Value == default(VehicleState))
            {
                return NotFound();
            }

            // log exit
            _logger.LogInformation("Exit detected in lane {Lane} at {Timestamp} of vehicle with license-number {LicenseNumber}.", msg.Lane, msg.Timestamp, msg.LicenseNumber);

            // update state
            var exitState = state.Value with { ExitTimestamp = msg.Timestamp };
            await _daprClient.SaveStateAsync(DAPR_STORE_NAME, msg.LicenseNumber, exitState);

            // handle possible speeding violation
            int violation = _speedingViolationCalculator.DetermineSpeedingViolationInKmh(exitState.EntryTimestamp, exitState.ExitTimestamp.Value);
            if (violation > 0)
            {
                _logger.LogInformation("Speeding violation detected ({Violation} km/h) of vehicle with license-number {LicenseNumber}.", violation, state.Value.LicenseNumber);

                var speedingViolation = new SpeedingViolation
                {
                    VehicleId = msg.LicenseNumber,
                    RoadId = _roadId,
                    ViolationInKmh = violation,
                    Timestamp = msg.Timestamp
                };

                // publish speeding violation (Dapr publish / subscribe)
                await _daprClient.PublishEventAsync("pubsub", "speedingviolations", speedingViolation);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing exit");
            throw;
        }
    }
}
