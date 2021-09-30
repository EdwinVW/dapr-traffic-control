namespace TrafficControlService.Models;

public record struct SpeedingViolation
{
    public string VehicleId { get; init; }
    public string RoadId { get; init; }
    public int ViolationInKmh { get; init; }
    public DateTime Timestamp { get; init; }
}
