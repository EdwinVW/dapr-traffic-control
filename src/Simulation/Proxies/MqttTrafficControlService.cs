namespace Simulation.Proxies;

public class MqttTrafficControlService : ITrafficControlService
{
    private IMqttClient _client;

    private MqttTrafficControlService(IMqttClient mqttClient)
    {
        _client = mqttClient;
    }

    public static async Task<MqttTrafficControlService> CreateAsync(int camNumber)
    {
        var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost";
        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();
        var mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttHost, 1883)
            .WithClientId($"camerasim{camNumber}")
            .Build();
        await client.ConnectAsync(mqttOptions, CancellationToken.None);
        return new MqttTrafficControlService(client);
    }

    public async Task SendVehicleEntryAsync(VehicleRegistered vehicleRegistered)
    {
        var eventJson = JsonSerializer.Serialize(vehicleRegistered);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic("trafficcontrol/entrycam")
            .WithPayload(Encoding.UTF8.GetBytes(eventJson))
            .WithAtMostOnceQoS()
            .Build();
        await _client.PublishAsync(message, CancellationToken.None);
    }

    public async Task SendVehicleExitAsync(VehicleRegistered vehicleRegistered)
    {
        var eventJson = JsonSerializer.Serialize(vehicleRegistered);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic("trafficcontrol/exitcam")
            .WithPayload(Encoding.UTF8.GetBytes(eventJson))
            .WithAtMostOnceQoS()
            .Build();
        await _client.PublishAsync(message, CancellationToken.None);
    }
}
