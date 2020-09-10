using System;

namespace TrafficControlService.Helpers
{
    public interface ISpeedingViolationCalculator
    {
        int DetermineSpeedingViolationInKmh(DateTime entryTimestamp, DateTime exitTimestamp);
        string GetRoadId();
    }
}