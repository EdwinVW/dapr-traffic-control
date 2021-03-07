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
    public class VehicleActor : Actor, IVehicleActor, IRemindable
    {
        private readonly ILogger<VehicleActor> _logger;
        public readonly ISpeedingViolationCalculator _speedingViolationCalculator;
        private readonly string _roadId;
        private readonly DaprClient _daprClient;

        public VehicleActor(ActorHost host, ILogger<VehicleActor> logger, DaprClient daprClient,
            ISpeedingViolationCalculator speedingViolationCalculator) : base(host)
        {
            _logger = logger;
            _daprClient = daprClient;
            _speedingViolationCalculator = speedingViolationCalculator;
            _roadId = _speedingViolationCalculator.GetRoadId();
        }

        public async Task RegisterEntry(VehicleRegistered msg)
        {
            try
            {
                _logger.LogInformation($"ENTRY detected in lane {msg.Lane} at " +
                    $"{msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}.");

                var vehicleState = new VehicleState
                {
                    LicenseNumber = msg.LicenseNumber,
                    EntryTimestamp = msg.Timestamp
                };
                await this.StateManager.SetStateAsync("VehicleState", vehicleState);

                //await RegisterReminderAsync("VehicleLost", null, TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(-1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterExit");
            }
        }

        public async Task RegisterExit(VehicleRegistered msg)
        {
            try
            {
                _logger.LogInformation($"EXIT detected in lane {msg.Lane} at " +
                    $"{msg.Timestamp.ToString("hh:mm:ss")} " +
                    $"of vehicle with license-number {msg.LicenseNumber}.");

                var vehicleState = await this.StateManager.GetStateAsync<VehicleState>("VehicleState");
                vehicleState.ExitTimestamp = msg.Timestamp;
                await this.StateManager.SaveStateAsync();

                //await UnregisterReminderAsync("VehicleLost");

                // handle possible speeding violation
                int violation = _speedingViolationCalculator.DetermineSpeedingViolationInKmh(
                    vehicleState.EntryTimestamp, vehicleState.ExitTimestamp);
                if (violation > 0)
                {
                    _logger.LogInformation($"Speeding violation detected ({violation} KMh) of vehicle " +
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
                _logger.LogError(ex, "Error in RegisterExit");
            }
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName == "CarLost")
            {
                var vehicleState = await this.StateManager.GetStateAsync<VehicleState>("VehicleState");

                _logger.LogInformation($"Lost track of vehicle with license-number {vehicleState.LicenseNumber}. " +
                    "Sending road-assistence.");

                // TODO: send road assistence!
            }
        }
    }
}