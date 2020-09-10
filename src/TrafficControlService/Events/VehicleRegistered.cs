using System;

namespace TrafficControlService.Events
{
    public class VehicleRegistered
    {
        public string LicenseNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}