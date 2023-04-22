namespace TrafficControlService.Models;

public record struct VehicleRegistered(int Lane, string LicenseNumber, DateTime Timestamp);