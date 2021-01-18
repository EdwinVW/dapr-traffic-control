using System;
using System.Collections.Generic;
using System.Threading;
using GovernmentService.Models;

namespace GovernmentService.Repositories
{
    public class InMemoryVehicleInfoRepository : IVehicleInfoRepository
    {
        private Random _rnd = new Random();

        private string[] _vehicleBrands = new string[] { "Mercedes", "Toyota", "Audi", "Volkswagen", "Seat", "Renault", "Skoda", "Kia" };

        private Dictionary<string, string[]> _models = new Dictionary<string, string[]>
        {
            { "Mercedes", new string[] { "A Class", "B Class", "C Class", "E Class", "SLS", "SLK" } },
            { "Toyota", new string[] { "Yaris", "Avensis", "Rav 4", "Prius", "Celica" } },
            { "Audi", new string[] { "A3", "A4", "A6", "A8", "Q5", "Q7" } },
            { "Volkswagen", new string[] { "Golf", "Pasat", "Tiguan", "Caddy" } },
            { "Seat", new string[] { "Leon", "Arona", "Ibiza", "Alhambra" } },
            { "Renault", new string[] { "Megane", "Clio", "Twingo", "Scenic", "Captur" } },
            { "Skoda", new string[] { "Octavia", "Fabia", "Superb", "Karoq", "Kodiaq" } },
            { "Kia", new string[] { "Picanto", "Rio", "Ceed", "XCeed", "Niro", "Sportage" } },
        };

        public VehicleInfo GetVehicleInfo(string licenseNumber)
        {
            // simulate slow IO
            Thread.Sleep(_rnd.Next(5, 200));

            string brand = GetRandomBrand();
            string model = GetRandomModel(brand);
            return new VehicleInfo
            {
                VehicleId = licenseNumber,
                Brand = brand,
                Model = model
            };
        }

        private string GetRandomBrand()
        {
            return _vehicleBrands[_rnd.Next(_vehicleBrands.Length)];
        }

        private string GetRandomModel(string brand)
        {
            string[] models = _models[brand];
            return models[_rnd.Next(models.Length)];
        }
    }
}