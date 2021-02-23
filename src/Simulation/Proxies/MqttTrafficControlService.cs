using System;
using System.Net.Http;
using System.Net.Mqtt;
using System.Text;
using System.Text.Json;
using Simulation.Events;

namespace Simulation.Proxies
{
    public class MqttTrafficControlService : ITrafficControlService
    {
        private readonly IMqttClient _client;

        public MqttTrafficControlService(int camNumber)
        {
            // connect to mqtt broker
            var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost";
            _client = MqttClient.CreateAsync(mqttHost, 1883).Result;
            var sessionState = _client.ConnectAsync(
                new MqttClientCredentials(clientId: $"camerasim{camNumber}")).Result;
        }

        public void SendVehicleEntry(VehicleRegistered vehicleRegistered)
        {
            var eventJson = JsonSerializer.Serialize(vehicleRegistered);
            var message = new MqttApplicationMessage("trafficcontrol/entrycam", Encoding.UTF8.GetBytes(eventJson));
            _client.PublishAsync(message, MqttQualityOfService.AtMostOnce).Wait();
        }

        public void SendVehicleExit(VehicleRegistered vehicleRegistered)
        {
            var eventJson = JsonSerializer.Serialize(vehicleRegistered);
            var message = new MqttApplicationMessage("trafficcontrol/exitcam", Encoding.UTF8.GetBytes(eventJson));
            _client.PublishAsync(message, MqttQualityOfService.AtMostOnce).Wait();
        }
    }
}