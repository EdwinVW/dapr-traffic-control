namespace Simulation.Events;

public record struct VehicleRegistered
{
    public int Lane { get; set; }
    public string LicenseNumber { get; set; }
    public DateTime Timestamp { get; set; }
}
