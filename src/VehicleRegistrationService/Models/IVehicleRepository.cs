namespace VehicleRegistrationService.Models;

public interface IVehicleInfoRepository
{
    VehicleInfo GetVehicleInfo(string licenseNumber);
}
