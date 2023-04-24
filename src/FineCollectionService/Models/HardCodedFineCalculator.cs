namespace FineCollectionService.Models;

public class HardCodedFineCalculator : IFineCalculator
{
    public int CalculateFine(string licenseKey, int violationInKmh)
    {
        if (licenseKey != "HX783-K2L7V-CRJ4A-5PN1G")
        {
            throw new InvalidOperationException("Invalid license-key specified.");
        }

        var fine = 9; // default administration fee
        
        switch (violationInKmh)
        {
            case < 5:
                fine += 18;
                break;
            case >= 5 and < 10:
                fine += 31;
                break;
            case >= 10 and < 15:
                fine += 64;
                break;
            case >= 15 and < 20:
                fine += 121;
                break;
            case >= 20 and < 25:
                fine += 174;
                break;
            case >= 25 and < 30:
                fine += 232;
                break;
            case >= 25 and < 35:
                fine += 297;
                break;
            case 35:
                fine += 372;
                break;
            default:
                // violation above 35 km/h will be determined by the prosecutor
                return 0;
        }

        return fine;
    }
}
