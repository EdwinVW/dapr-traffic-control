using System;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using TrafficControlService.DomainServices;
using TrafficControlService.Events;
using TrafficControlService.Models;

namespace TrafficControlService.Actors
{
    public class VehicleActor : Actor, IVehicleActor
    {
        public readonly ISpeedingViolationCalculator _speedingViolationCalculator;
        private readonly string _roadId;
        private readonly DaprClient _daprClient;

        public VehicleActor(ActorHost host, DaprClient daprClient,ISpeedingViolationCalculator speedingViolationCalculator) : base(host)
        {
            _daprClient = daprClient;
            _speedingViolationCalculator = speedingViolationCalculator;
            _roadId = _speedingViolationCalculator.GetRoadId();
        }

        public async Task RegisterEntryAsync(VehicleRegistered msg)
        {
            try
            {
                Logger.LogInformation($"ENTRY detected in lane {msg.Lane} at " +
                    $"{msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}.");

                // store vehicle state
                var vehicleState = new VehicleState
                {
                    LicenseNumber = msg.LicenseNumber,
                    EntryTimestamp = msg.Timestamp
                };
                await this.StateManager.SetStateAsync("VehicleState", vehicleState);

                // register a timer for cars that enter but don't exit within 20 seconds
                // (they might have broken down and need road assistence)
                //await RegisterTimerAsync("VehicleLost", "VehicleLost", null, TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(20));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in RegisterEntry");
            }
        }

        public async Task RegisterExitAsync(VehicleRegistered msg)
        {
            try
            {
                Logger.LogInformation($"EXIT detected in lane {msg.Lane} at " +
                    $"{msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}.");

                // remove lost vehicle timer
                //await UnregisterTimerAsync("VehicleLost");

                // get vehicle state
                var vehicleState = await this.StateManager.GetStateAsync<VehicleState>("VehicleState");
                vehicleState.ExitTimestamp = msg.Timestamp;
                await this.StateManager.SaveStateAsync();

                // handle possible speeding violation
                int violation = _speedingViolationCalculator.DetermineSpeedingViolationInKmh(
                    vehicleState.EntryTimestamp, vehicleState.ExitTimestamp);
                if (violation > 0)
                {
                    Logger.LogInformation($"Speeding violation detected ({violation} KMh) of vehicle " +
                        $"with license-number {vehicleState.LicenseNumber}.");

                    var speedingViolation = new SpeedingViolation
                    {
                        VehicleId = msg.LicenseNumber,
                        RoadId = _roadId,
                        ViolationInKmh = violation,
                        Timestamp = msg.Timestamp
                    };

                    // publish speedingviolation (Dapr publish / subscribe)
                    await _daprClient.PublishEventAsync("pubsub", "collectfine", speedingViolation);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in RegisterExit");
            }
        }

        public async Task VehicleLost(byte[] state)
        {
            // remove lost vehicle timer
            //await UnregisterTimerAsync("VehicleLost");

            var vehicleState = await this.StateManager.GetStateAsync<VehicleState>("VehicleState");

            Logger.LogInformation($"Lost track of vehicle with license-number {vehicleState.LicenseNumber}. " +
                "Sending road-assistence.");

            // send road assistence ...
        }
    }
}