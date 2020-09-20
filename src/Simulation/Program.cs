using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation
{
    class Program
    {
        static void Main(string[] args)
        {
            var cam1 = new CameraSimulation();
            var cam2 = new CameraSimulation();
            var cam3 = new CameraSimulation();

            Task.Run(() => cam1.Start(1));
            Task.Run(() => cam2.Start(2));
            Task.Run(() => cam3.Start(3));

            Task.Run(() => Thread.Sleep(Timeout.Infinite)).Wait();
        }
    }
}
