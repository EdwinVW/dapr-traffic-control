namespace TrafficControlService.Events;

public record struct VehicleRegistered
{
    public int Lane { get; init; }
    public string LicenseNumber { get; init; }
    public DateTime Timestamp { get; init; }
}
