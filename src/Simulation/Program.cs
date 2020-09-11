using System;
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
            Task.Run(() => cam1.Start());
            Task.Run(() => cam2.Start());
            Task.Run(() => cam3.Start());
            Console.ReadLine();
        }
    }
}
