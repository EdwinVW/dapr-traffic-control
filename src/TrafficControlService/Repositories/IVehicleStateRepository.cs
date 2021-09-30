namespace TrafficControlService.Repositories;

public interface IVehicleStateRepository
{
    Task SaveVehicleStateAsync(VehicleState vehicleState);
    Task<VehicleState?> GetVehicleStateAsync(string licenseNumber);
}
