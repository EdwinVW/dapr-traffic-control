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
        private string _expectedAPIKey;
        private Random _rnd;

        public RDWController(ILogger<RDWController> logger,
            IVehicleInfoRepository vehicleInfoRepository, DaprClient daprClient)
        {
            _logger = logger;
            _vehicleInfoRepository = vehicleInfoRepository;
            _rnd = new Random();

            // specify secret-store to use based on hosting environment
            string runningInK8s = (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "false").ToLowerInvariant();
            string secretStoreName = runningInK8s == "true" ? "kubernetes" : "secret-store-file";

            // get API key
            var apiKeySecret = daprClient.GetSecretAsync(secretStoreName, "rdw-api-key",
                new Dictionary<string,string>{ { "namespace", "dapr-trafficcontrol" } }).Result;
            _expectedAPIKey = apiKeySecret["rdw-api-key"];
        }

        [HttpGet("rdw/{apikey}/vehicle/{licenseNumber}")]
        public ActionResult<VehicleInfo> GetVehicleDetails(string apiKey, string licenseNumber)
        {
            if (apiKey != _expectedAPIKey)
            {
                return Unauthorized();
            }

            _logger.LogInformation($"RDW: Retrieving vehicle-info for licensenumber {licenseNumber}");
            VehicleInfo info = _vehicleInfoRepository.GetVehicleInfo(licenseNumber);
            return info;
        }
    }
}
