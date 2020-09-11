using System;

namespace GovernmentService.Events
{
    public class SpeedingViolationDetected
    {
        public string VehicleId { get; set; }
        public string RoadId { get; set; }
        public int ViolationInKmh { get; set; }
        public DateTime Timestamp { get; set; }
    }
}