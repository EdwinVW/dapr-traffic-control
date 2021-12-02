namespace TrafficControlService.Models;

public record struct VehicleState
{
    public string LicenseNumber { get; init; }
    public DateTime EntryTimestamp { get; init; }
    public DateTime? ExitTimestamp { get; init; }

    public VehicleState(string licenseNumber, DateTime entryTimestamp, DateTime? exitTimestamp = null)
    {
        this.LicenseNumber = licenseNumber;
        this.EntryTimestamp = entryTimestamp;
        this.ExitTimestamp = exitTimestamp;
    }
}
