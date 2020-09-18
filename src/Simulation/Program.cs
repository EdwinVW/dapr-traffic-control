using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cam1 = new CameraSimulation();
            var cam2 = new CameraSimulation();
            var cam3 = new CameraSimulation();
            
            await Task.Run(() => cam1.Start());
            await Task.Run(() => cam2.Start());
            await Task.Run(() => cam3.Start());

            await Task.Run(() => Thread.Sleep(Timeout.Infinite));
        }
    }
}
