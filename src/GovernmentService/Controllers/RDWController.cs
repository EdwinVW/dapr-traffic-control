using System;
using System.Collections.Generic;
using Dapr.Client;
using GovernmentService.Models;
using GovernmentService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovernmentService.Controllers
{
    [ApiController]
    public class RDWController : ControllerBase
    {
        private readonly ILogger<RDWController> _logger;
        private readonly IVehicleInfoRepository _vehicleInfoRepository;
        private readonly Random _rnd;

        public RDWController(ILogger<RDWController> logger,
            IVehicleInfoRepository vehicleInfoRepository, DaprClient daprClient)
        {
            _logger = logger;
            _vehicleInfoRepository = vehicleInfoRepository;
            _rnd = new Random();
        }

        [HttpGet("rdw/vehicle/{licenseNumber}")]
        public ActionResult<VehicleInfo> GetVehicleDetails(string licenseNumber)
        {
            _logger.LogInformation($"RDW: Retrieving vehicle-info for licensenumber {licenseNumber}");
            VehicleInfo info = _vehicleInfoRepository.GetVehicleInfo(licenseNumber);
            return info;
        }
    }
}
