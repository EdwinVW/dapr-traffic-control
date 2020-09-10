namespace TestClient.Messages
{
    public class SpeedingViolationDetected
    {
        public string VehicleId { get; set; }
        public string RoadId { get; set; }
        public int ViolationInKmh { get; set; }
    }
}