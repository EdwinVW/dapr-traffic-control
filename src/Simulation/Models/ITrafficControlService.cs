namespace Simulation.Models;

public interface ITrafficControlService
{
    public Task SendVehicleEntryAsync(VehicleRegistered vehicleRegistered);
    public Task SendVehicleExitAsync(VehicleRegistered vehicleRegistered);
}
