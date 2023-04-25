namespace TrafficControlService.Models;

public class SpeedingViolationCalculator : ISpeedingViolationCalculator
{
    private readonly string _roadId;
    private readonly int _sectionLengthInKm;
    private readonly int _maxAllowedSpeedInKmh;
    private readonly int _legalCorrectionInKmh;

    public SpeedingViolationCalculator(string roadId, int sectionLengthInKm, int maxAllowedSpeedInKmh, int legalCorrectionInKmh)
    {
        _roadId = roadId;
        _sectionLengthInKm = sectionLengthInKm;
        _maxAllowedSpeedInKmh = maxAllowedSpeedInKmh;
        _legalCorrectionInKmh = legalCorrectionInKmh;
    }

    public int DetermineSpeedingViolationInKmh(DateTime entryTimestamp, DateTime exitTimestamp)
    {
        var elapsedMinutes = exitTimestamp.Subtract(entryTimestamp).TotalSeconds; // 1 sec. == 1 min. in simulation
        var avgSpeedInKmh = Math.Round((_sectionLengthInKm / elapsedMinutes) * 60);
        var violation = Convert.ToInt32(avgSpeedInKmh - _maxAllowedSpeedInKmh - _legalCorrectionInKmh);
        return violation;
    }

    public string GetRoadId() => _roadId;
}
