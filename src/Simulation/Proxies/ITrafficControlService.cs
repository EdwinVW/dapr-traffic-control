using System.Threading.Tasks;
using Simulation.Events;

namespace Simulation.Proxies
{
    public interface ITrafficControlService
    {
        public Task SendVehicleEntry(VehicleRegistered vehicleRegistered);
        public Task SendVehicleExit(VehicleRegistered vehicleRegistered);
    }
}