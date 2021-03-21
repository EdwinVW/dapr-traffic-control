﻿//#define USE_ACTORMODEL

/* 
This controller contains 2 implementations of the TrafficControl functionality: a basic 
implementation and an actor-model based implementation.

The code for the basic implementation is in this controller. The actor-model implementation 
resides in the Vehicle actor (./Actors/VehicleActor.cs).

To switch between the two implementatons, you need to use the USE_ACTORMODEL symbol at
the top of this file. If you comment the #define USE_ACTORMODEL statement, the basic 
implementation is used. Uncomment this statement to use the Actormodel implementation.
*/

using System;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TrafficControlService.Events;
using TrafficControlService.DomainServices;
using TrafficControlService.Models;
using TrafficControlService.Repositories;
using Dapr.Actors;
using Dapr.Actors.Client;

namespace TrafficControlService.Controllers
{
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
        public async Task<ActionResult> VehicleEntry(VehicleRegistered msg)
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
        public async Task<ActionResult> VehicleExit(VehicleRegistered msg, [FromServices] DaprClient daprClient)
        {
            try
            {
                // get vehicle state
                var state = await _vehicleStateRepository.GetVehicleStateAsync(msg.LicenseNumber);
                if (state == null)
                {
                    return NotFound();
                }

                // log exit
                _logger.LogInformation($"EXIT detected in lane {msg.Lane} at {msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}.");

                // update state
                state.ExitTimestamp = msg.Timestamp;
                await _vehicleStateRepository.SaveVehicleStateAsync(state);

                // handle possible speeding violation
                int violation = _speedingViolationCalculator.DetermineSpeedingViolationInKmh(state.EntryTimestamp, state.ExitTimestamp);
                if (violation > 0)
                {
                    _logger.LogInformation($"Speeding violation detected ({violation} KMh) of vehicle" +
                        $"with license-number {state.LicenseNumber}.");

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing EXIT");
                return StatusCode(500);
            }
        }

#else

        [HttpPost("entrycam")]
        public async Task<ActionResult> VehicleEntry(VehicleRegistered msg)
        {
            try
            {
                var actorId = new ActorId(msg.LicenseNumber);
                var proxy = ActorProxy.Create(actorId, "VehicleActor");
                await proxy.InvokeMethodAsync("RegisterEntry", msg);
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("exitcam")]
        public async Task<ActionResult> VehicleExit(VehicleRegistered msg)
        {
            try
            {
                var actorId = new ActorId(msg.LicenseNumber);
                var proxy = ActorProxy.Create(actorId, "VehicleActor");
                await proxy.InvokeMethodAsync("RegisterExit", msg);
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }
        }

#endif

    }
}
