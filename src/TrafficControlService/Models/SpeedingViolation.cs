namespace TrafficControlService.Models;

public record struct SpeedingViolation(string VehicleId, string RoadId, int ViolationInKmh, DateTime Timestamp);