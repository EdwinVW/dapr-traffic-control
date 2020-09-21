using GovernmentService.Models;
using GovernmentService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovernmentService.Controllers
{
    [ApiController]
    public class RDWController : ControllerBase
    {
        private const string SUPER_SECRET_API_KEY = "A6k9D42L061Fx4Rm2K8";
        private readonly ILogger<RDWController> _logger;
        private readonly IVehicleInfoRepository _vehicleInfoRepository;

        public RDWController(ILogger<RDWController> logger, IVehicleInfoRepository vehicleInfoRepository)
        {
            _logger = logger;
            _vehicleInfoRepository = vehicleInfoRepository;
        }

        [HttpGet("rdw/{apikey}/vehicle/{licenseNumber}")]
        public ActionResult<VehicleInfo> GetVehicleDetails(string apiKey, string licenseNumber)
        {
            if (apiKey != SUPER_SECRET_API_KEY)
            {
                return Unauthorized();
            }

            _logger.LogInformation($"RDW: Retrieving vehicle-info for licensenumber {licenseNumber}");
            VehicleInfo info = _vehicleInfoRepository.GetVehicleInfo(licenseNumber);
            return info;
        }
    }
}
