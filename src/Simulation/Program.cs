using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation
{
    class Program
    {
        static void Main(string[] args)
        {
            int lanes = 3;
            CameraSimulation[] cameras = new CameraSimulation[lanes];
            for (var i = 0; i < lanes; i++)
            {
                cameras[i] = new CameraSimulation(i + 1);
            }
            Parallel.ForEach(cameras, cam => cam.Start());

            Task.Run(() => Thread.Sleep(Timeout.Infinite)).Wait();
        }
    }
}
