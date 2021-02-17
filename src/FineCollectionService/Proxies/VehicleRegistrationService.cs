using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FineCollectionService.Models;

namespace FineCollectionService.Proxies
{
    public class VehicleRegistrationService
    {
        private HttpClient _httpClient;
        private JsonSerializerOptions _serializerOptions;

        public VehicleRegistrationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<VehicleInfo> GetVehicleInfo(string licenseNumber)
        {
            return await _httpClient.GetFromJsonAsync<VehicleInfo>(
                $"vehicleinfo/{licenseNumber}", _serializerOptions);
        }       
    }
}