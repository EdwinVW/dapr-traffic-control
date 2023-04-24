namespace VehicleRegistrationService.Models;

public interface IVehicleInfoRepository
{
    Task<VehicleInfo> GetVehicleInfo(string licenseNumber);
}
