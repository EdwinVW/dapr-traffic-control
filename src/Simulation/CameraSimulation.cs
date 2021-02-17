using System;
using System.Net.Mqtt;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Simulation.Events;

namespace Simulation
{
    public class CameraSimulation
    {
        private Random _rnd;
        private int _camNumber;
        private int _minEntryDelayInMS = 50;
        private int _maxEntryDelayInMS = 5000;
        private int _minExitDelayInS = 4;
        private int _maxExitDelayInS = 10;

        public CameraSimulation(int camNumber)
        {
            _camNumber = camNumber;
        }

        public void Start()
        {
            Console.WriteLine($"Start camera {_camNumber} simulation.");

            // initialize state
            _rnd = new Random();

            // connect to mqtt broker
            var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost";
            var client = MqttClient.CreateAsync(mqttHost, 1883).Result;
            var sessionState = client.ConnectAsync(
                new MqttClientCredentials(clientId: $"camerasim{_camNumber}")).Result;

            while (true)
            {
                try
                {
                    // simulate entry
                    TimeSpan entryDelay = TimeSpan.FromMilliseconds(_rnd.Next(_minEntryDelayInMS, _maxEntryDelayInMS) + _rnd.NextDouble());
                    Task.Delay(entryDelay).Wait();

                    Task.Run(() =>
                    {
                        // simulate entry
                        DateTime entryTimestamp = DateTime.Now;
                        var @event = new VehicleRegistered
                        {
                            Lane = _camNumber,
                            LicenseNumber = GenerateRandomLicenseNumber(),
                            Timestamp = entryTimestamp
                        };
                        var eventJson = JsonSerializer.Serialize(@event);
                        var message = new MqttApplicationMessage("trafficcontrol/entrycam", Encoding.UTF8.GetBytes(eventJson));
                        client.PublishAsync(message, MqttQualityOfService.AtMostOnce).Wait();
                        Console.WriteLine($"Simulated ENTRY of vehicle with license-number {@event.LicenseNumber} in lane {@event.Lane}");

                        // simulate exit
                        TimeSpan exitDelay = TimeSpan.FromSeconds(_rnd.Next(_minExitDelayInS, _maxExitDelayInS) + _rnd.NextDouble());
                        Task.Delay(exitDelay).Wait();
                        @event.Timestamp = DateTime.Now;
                        @event.Lane = _rnd.Next(1, 4);
                        eventJson = JsonSerializer.Serialize(@event);
                        message = new MqttApplicationMessage("trafficcontrol/exitcam", Encoding.UTF8.GetBytes(eventJson));
                        client.PublishAsync(message, MqttQualityOfService.AtMostOnce).Wait();
                        Console.WriteLine($"Simulated EXIT of vehicle with license-number {@event.LicenseNumber} in lane {@event.Lane}");
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        #region Private helper methods

        private string _validLicenseNumberChars = "DFGHJKLNPRSTXYZ";

        private string GenerateRandomLicenseNumber()
        {
            int type = _rnd.Next(1, 9);
            string kenteken = null;
            switch (type)
            {
                case 1: // 99-AA-99
                    kenteken = string.Format("{0:00}-{1}-{2:00}", _rnd.Next(1, 99), GenerateRandomCharacters(2), _rnd.Next(1, 99));
                    break;
                case 2: // AA-99-AA
                    kenteken = string.Format("{0}-{1:00}-{2}", GenerateRandomCharacters(2), _rnd.Next(1, 99), GenerateRandomCharacters(2));
                    break;
                case 3: // AA-AA-99
                    kenteken = string.Format("{0}-{1}-{2:00}", GenerateRandomCharacters(2), GenerateRandomCharacters(2), _rnd.Next(1, 99));
                    break;
                case 4: // 99-AA-AA
                    kenteken = string.Format("{0:00}-{1}-{2}", _rnd.Next(1, 99), GenerateRandomCharacters(2), GenerateRandomCharacters(2));
                    break;
                case 5: // 99-AAA-9
                    kenteken = string.Format("{0:00}-{1}-{2}", _rnd.Next(1, 99), GenerateRandomCharacters(3), _rnd.Next(1, 10));
                    break;
                case 6: // 9-AAA-99
                    kenteken = string.Format("{0}-{1}-{2:00}", _rnd.Next(1, 9), GenerateRandomCharacters(3), _rnd.Next(1, 10));
                    break;
                case 7: // AA-999-A
                    kenteken = string.Format("{0}-{1:000}-{2}", GenerateRandomCharacters(2), _rnd.Next(1, 999), GenerateRandomCharacters(1));
                    break;
                case 8: // A-999-AA
                    kenteken = string.Format("{0}-{1:000}-{2}", GenerateRandomCharacters(1), _rnd.Next(1, 999), GenerateRandomCharacters(2));
                    break;
            }

            return kenteken;
        }

        private string GenerateRandomCharacters(int aantal)
        {
            char[] chars = new char[aantal];
            for (int i = 0; i < aantal; i++)
            {
                chars[i] = _validLicenseNumberChars[_rnd.Next(_validLicenseNumberChars.Length - 1)];
            }
            return new string(chars);
        }

        #endregion
    }
}