using GovernmentService.Models;

namespace GovernmentService.Repositories
{
    public interface IVehicleInfoRepository
    {
        VehicleInfo GetVehicleInfo(string licenseNumber);
    }
}