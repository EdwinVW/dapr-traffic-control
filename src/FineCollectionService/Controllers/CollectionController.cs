﻿namespace FineCollectionService.Controllers;

[ApiController]
[Route("")]
public class CollectionController : ControllerBase
{
    private static string? _fineCalculatorLicenseKey;
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

        // set fine calculator component license-key
        if (_fineCalculatorLicenseKey is null)
        {
            bool useKubernetesSecrets = Convert.ToBoolean(Environment.GetEnvironmentVariable("USE_KUBERNETES_SECRETS") ?? "false");
            string secretName = Environment.GetEnvironmentVariable("FINE_CALCULATOR_LICENSE_SECRET_NAME") ?? "finecalculator.licensekey";
            var metadata = new Dictionary<string, string> { { "namespace", "dapr-trafficcontrol" } };
            if (useKubernetesSecrets)
            {
                var k8sSecrets = daprClient.GetSecretAsync(
                    "kubernetes", "trafficcontrol-secrets", metadata).Result;
                _fineCalculatorLicenseKey = k8sSecrets[secretName];
            }
            else
            {
                var secrets = daprClient.GetSecretAsync(
                    "trafficcontrol-secrets", secretName, metadata).Result;
                _fineCalculatorLicenseKey = secrets[secretName];
            }
        }
    }

    [Topic("pubsub", "speedingviolations", "deadletters", false)]
    [Route("collectfine")]
    [HttpPost]
    public async Task<ActionResult> CollectFine(SpeedingViolation speedingViolation, [FromServices] DaprClient daprClient)
    {
        decimal fine = _fineCalculator.CalculateFine(_fineCalculatorLicenseKey!, speedingViolation.ViolationInKmh);

        // get owner info (Dapr service invocation)
        var vehicleInfo = _vehicleRegistrationService.GetVehicleInfo(speedingViolation.VehicleId).Result;

        // log fine
        string fineString = fine == 0 ? "TBD by the prosecutor" : $"{fine} Euro";
        _logger.LogInformation("Sent speeding ticket to {Owner}. Road: {Road}, License number: {LicenceNumber}, Vehicle: {Make} {Model}, Violation: {ViolationInKmh} Km/h, Fine: {Fine}, On: {Timestamp}.", vehicleInfo.OwnerName, speedingViolation.RoadId, speedingViolation.VehicleId, vehicleInfo.Make, vehicleInfo.Model, speedingViolation.ViolationInKmh, fineString, speedingViolation.Timestamp);

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

    [Topic("pubsub", "deadletters")]
    [Route("deadletters")]
    [HttpPost]
    public ActionResult HandleDeadLetter(object message)
    {
        _logger.LogError("The service was not able to handle a CollectFine message.");

        try
        {
            var messageJson = JsonSerializer.Serialize<object>(message);
            _logger.LogInformation("Unhandled message content: {Message}", messageJson);
        }
        catch
        {
            _logger.LogError("Unhandled message's payload could not be deserialized.");
        }

        return Ok();
    }
}
