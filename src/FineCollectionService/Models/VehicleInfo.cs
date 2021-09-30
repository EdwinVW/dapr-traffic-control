namespace FineCollectionService.Models;

public record struct VehicleInfo
{
    public string VehicleId { get; init; }
    public string Brand { get; init; }
    public string Model { get; init; }
    public string OwnerName { get; init; }
    public string OwnerEmail { get; init; }
}
