using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using TestClient.Messages;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var msg = new SpeedingViolationDetected { VehicleId = "ABC", RoadId = "A1", ViolationInKmh = 15 };
            var daprClient = new DaprClientBuilder().Build();
            Console.Write("Sending message...");
            daprClient.PublishEventAsync<SpeedingViolationDetected>("pubsub-nats", "cjib", msg).Wait();
            Console.WriteLine("Done.");
        }
    }
}
