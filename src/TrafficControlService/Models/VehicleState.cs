namespace TrafficControlService.Models;

public record struct VehicleState(string LicenseNumber, DateTime EntryTimestamp, DateTime? ExitTimestamp = null);
