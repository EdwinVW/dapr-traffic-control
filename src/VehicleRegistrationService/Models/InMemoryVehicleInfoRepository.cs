namespace VehicleRegistrationService.Models;

public class InMemoryVehicleInfoRepository : IVehicleInfoRepository
{
    private Random _rnd;

    private PersonNameGenerator _nameGenerator;

    private readonly string[] _vehicleBrands = new[] {
        "Mercedes",
        "Toyota",
        "Audi",
        "Volkswagen",
        "Seat",
        "Renault",
        "Škoda",
        "Kia",
        "Citroën",
        "Suzuki",
        "Mitsubishi",
        "Fiat",
        "Opel"
    };

    private Dictionary<string, string[]> _models = new Dictionary<string, string[]>
        {
            { "Mercedes", new[] { "A Class", "B Class", "C Class", "E Class", "S Class" } },
            { "Toyota", new[] { "Yaris", "Avensis", "Rav 4", "Prius", "Celica" } },
            { "Audi", new[] { "A3", "A4", "A5", "A6", "A7", "A8", "Q3", "Q5", "Q7" } },
            { "Volkswagen", new[] { "Golf", "Passat", "Tiguan", "Caddy" } },
            { "Seat", new[] { "Leon", "Arona", "Ibiza", "Alhambra" } },
            { "Renault", new[] { "Megane", "Clio", "Twingo", "Scenic", "Captur" } },
            { "Škoda", new[] { "Octavia", "Fabia", "Superb", "Karoq", "Kodiaq" } },
            { "Kia", new[] { "Picanto", "Rio", "Ceed", "XCeed", "Niro", "Sportage" } },
            { "Citroën", new[] { "C1", "C2", "C3", "C4", "C4 Cactus", "Berlingo" } },
            { "Suzuki", new[] { "Ignis", "Swift", "Vitara", "S-Cross", "Swace", "Jimny" } },
            { "Mitsubishi", new[] { "Space Star", "ASX", "Eclipse Cross", "Outlander" } },
            { "Ford", new[] { "Focus", "Ka", "C-Max", "Fusion", "Fiesta", "Mondeo", "Kuga" } },
            { "BMW", new[] { "1 Series", "2 Series", "3 Series", "5 Series", "7 Series", "X5" } },
            { "Fiat", new[] { "500", "Panda", "Punto", "Tipo", "Multipla" } },
            { "Opel", new[] { "Karl", "Corsa", "Astra", "Crossland X", "Insignia" } }
        };

    public InMemoryVehicleInfoRepository()
    {
        _rnd = new Random();
        _nameGenerator = new PersonNameGenerator(_rnd);
    }

    public VehicleInfo GetVehicleInfo(string licenseNumber)
    {
        // simulate slow IO
        Thread.Sleep(_rnd.Next(5, 200));

        // get random vehicle info
        var make = GetRandomMake();
        var model = GetRandomModel(make);

        // get random owner info
        var ownerName = _nameGenerator.GenerateRandomFirstAndLastName();
        var ownerEmail = $"{ownerName.ToLowerInvariant().Replace(' ', '.')}@outlook.com";

        // return info
        return new VehicleInfo
        {
            VehicleId = licenseNumber,
            Make = make,
            Model = model,
            OwnerName = ownerName,
            OwnerEmail = ownerEmail
        };
    }

    private string GetRandomMake()
    {
        return _vehicleBrands[_rnd.Next(_vehicleBrands.Length)];
    }

    private string GetRandomModel(string brand)
    {
        var models = _models[brand];
        return models[_rnd.Next(models.Length)];
    }
}
