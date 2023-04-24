namespace VehicleRegistrationService.Controllers;

[ApiController]
[Route("[controller]")]
public class VehicleInfoController : ControllerBase
{
    private readonly ILogger<VehicleInfoController> _logger;
    private readonly IVehicleInfoRepository _vehicleInfoRepository;

    public VehicleInfoController(IVehicleInfoRepository vehicleInfoRepository, ILogger<VehicleInfoController> logger)
    {
        _vehicleInfoRepository = vehicleInfoRepository;
        _logger = logger;
    }

    [HttpGet("{licenseNumber}")]
    public async Task<ActionResult<VehicleInfo>> GetVehicleInfo(string licenseNumber)
    {
        _logger.LogInformation("Retrieving vehicle-info for license number {licenseNumber}", licenseNumber);

        return await _vehicleInfoRepository.GetVehicleInfo(licenseNumber);
    }
}
