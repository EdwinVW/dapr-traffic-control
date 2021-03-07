using System.Threading.Tasks;
using Dapr.Actors;
using TrafficControlService.Events;

namespace TrafficControlService.Actors
{
    public interface IVehicleActor : IActor
    {
        public Task RegisterEntry(VehicleRegistered msg);
        public Task RegisterExit(VehicleRegistered msg);
    }
}