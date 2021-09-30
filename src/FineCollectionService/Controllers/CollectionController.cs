namespace FineCollectionService.Controllers;

[ApiController]
[Route("")]
public class CollectionController : ControllerBase
{
    private static string? _fineCalculatorLicenseKey = null;
    private readonly ILogger<CollectionController> _logger;
    private readonly IFineCalculator _fineCalculator;
    private readonly VehicleRegistrationService _vehicleRegistrationService;

    public CollectionController(ILogger<CollectionController> logger,
        IFineCalculator fineCalculator, VehicleRegistrationService vehicleRegistrationService,
        DaprClient daprClient)
    {
        _logger = logger;
        _fineCalculator = fineCalculator;
        _vehicleRegistrationService = vehicleRegistrationService;

        // set finecalculator component license-key
        if (_fineCalculatorLicenseKey == null)
        {
            bool runningInK8s = Convert.ToBoolean(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "false");
            var metadata = new Dictionary<string, string> { { "namespace", "dapr-trafficcontrol" } };
            if (runningInK8s)
            {
                var k8sSecrets = daprClient.GetSecretAsync(
                    "kubernetes", "trafficcontrol-secrets", metadata).Result;
                _fineCalculatorLicenseKey = k8sSecrets["finecalculator.licensekey"];
            }
            else
            {
                var secrets = daprClient.GetSecretAsync(
                    "trafficcontrol-secrets", "finecalculator.licensekey", metadata).Result;
                _fineCalculatorLicenseKey = secrets["finecalculator.licensekey"];
            }
        }
    }

    [Topic("pubsub", "speedingviolations")]
    [Route("collectfine")]
    [HttpPost()]
    public async Task<ActionResult> CollectFine(SpeedingViolation speedingViolation, [FromServices] DaprClient daprClient)
    {
        decimal fine = _fineCalculator.CalculateFine(_fineCalculatorLicenseKey!, speedingViolation.ViolationInKmh);

        // get owner info (Dapr service invocation)
        var vehicleInfo = _vehicleRegistrationService.GetVehicleInfo(speedingViolation.VehicleId).Result;

        // log fine
        string fineString = fine == 0 ? "tbd by the prosecutor" : $"{fine} Euro";
        _logger.LogInformation($"Sent speeding ticket to {vehicleInfo.OwnerName}. " +
            $"Road: {speedingViolation.RoadId}, Licensenumber: {speedingViolation.VehicleId}, " +
            $"Vehicle: {vehicleInfo.Brand} {vehicleInfo.Model}, " +
            $"Violation: {speedingViolation.ViolationInKmh} Km/h, Fine: {fineString}, " +
            $"On: {speedingViolation.Timestamp.ToString("dd-MM-yyyy")} " +
            $"at {speedingViolation.Timestamp.ToString("hh:mm:ss")}.");

        // send fine by email (Dapr output binding)
        var body = EmailUtils.CreateEmailBody(speedingViolation, vehicleInfo, fineString);
        var metadata = new Dictionary<string, string>
        {
            ["emailFrom"] = "noreply@cfca.gov",
            ["emailTo"] = vehicleInfo.OwnerEmail,
            ["subject"] = $"Speeding violation on the {speedingViolation.RoadId}"
        };
        await daprClient.InvokeBindingAsync("sendmail", "create", body, metadata);

        return Ok();
    }
}
