namespace TrafficControlService.Models;

public interface IVehicleActor : IActor
{
    public Task RegisterEntryAsync(VehicleRegistered msg);
    public Task RegisterExitAsync(VehicleRegistered msg);
}
