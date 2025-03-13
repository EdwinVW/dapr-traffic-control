namespace FineCollectionService.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class EmailUtils
{
    private static string? _templateCache;

    public static string CreateEmailBody(
        SpeedingViolation speedingViolation,
        VehicleInfo vehicleInfo,
        string fine)
    {
        // Load template once and cache it
        _templateCache ??= File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "EmailTemplate.html"));

        // Create replacement dictionary
        var replacements = new Dictionary<string, string>
        {
            ["{Date}"] = DateTime.Now.ToLongDateString(),
            ["{OwnerName}"] = vehicleInfo.OwnerName,
            ["{VehicleId}"] = vehicleInfo.VehicleId,
            ["{Brand}"] = vehicleInfo.Brand,
            ["{Model}"] = vehicleInfo.Model,
            ["{RoadId}"] = speedingViolation.RoadId,
            ["{ViolationDate}"] = speedingViolation.Timestamp.ToString("dd-MM-yyyy"),
            ["{ViolationTime}"] = speedingViolation.Timestamp.ToString("hh:mm:ss"),
            ["{ViolationSpeed}"] = $"{speedingViolation.ViolationInKmh} KMh",
            ["{Fine}"] = fine
        };

        // Replace all placeholders
        return replacements.Aggregate(_templateCache, 
            (current, replacement) => current.Replace(replacement.Key, replacement.Value));
    }
}
