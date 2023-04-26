namespace Simulation.Models;

public class CameraSimulation
{
    private readonly HttpClient _client;
    private Random _random;
    private int _cameraNumber;
    private int _minEntryDelayInMilliseconds = 50;
    private int _maxEntryDelayInMilliseconds = 5000;
    private int _minExitDelayInSeconds = 4;
    private int _maxExitDelayInSeconds = 10;

    public CameraSimulation(int cameraNumber, HttpClient client)
    {
        _random = new Random();
        _cameraNumber = cameraNumber;
        _client = client;
    }

    public async Task Start()
    {
        Console.WriteLine($"Start camera {_cameraNumber} simulation.");

        while (true)
        {
            try
            {
                // simulate entry
                var entryDelay = TimeSpan.FromMilliseconds(_random.Next(_minEntryDelayInMilliseconds, _maxEntryDelayInMilliseconds) + _random.NextDouble());
                await Task.Delay(entryDelay);

                await Task.Run(async () =>
                {
                    // simulate entry
                    var entryTimestamp = DateTime.Now;
                    var vehicleRegistered = new VehicleRegistered
                    {
                        Lane = _cameraNumber,
                        LicenseNumber = GenerateRandomLicenseNumber(),
                        Timestamp = entryTimestamp
                    };
                    var entry = await _client.PostAsJsonAsync("entrycam", vehicleRegistered);
                    entry.EnsureSuccessStatusCode();

                    Console.WriteLine($"Simulated ENTRY of vehicle with license number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");

                    // simulate exit
                    var exitDelay = TimeSpan.FromSeconds(_random.Next(_minExitDelayInSeconds, _maxExitDelayInSeconds) + _random.NextDouble());
                    await Task.Delay(exitDelay);
                    vehicleRegistered.Timestamp = DateTime.Now;
                    vehicleRegistered.Lane = _random.Next(1, 4);
                    var exit = await _client.PostAsJsonAsync("exitcam", vehicleRegistered);
                    exit.EnsureSuccessStatusCode();

                    Console.WriteLine($"Simulated EXIT of vehicle with license number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Camera {_cameraNumber} error: {ex.Message}");
            }
        }
    }

    private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string _digits = "0123456789";

    private string GenerateRandomLicenseNumber() => $"{GenerateRandomCharacters(2, _chars)}-{GenerateRandomCharacters(5, _digits)}";

    private string GenerateRandomCharacters(int count, string validChars)
    {
        var chars = new char[count];
        for (var i = 0; i < count; i++)
        {
            chars[i] = validChars[_random.Next(validChars.Length - 1)];
        }
        return new string(chars);
    }
}
