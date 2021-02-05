using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TrafficControlService.Models;

namespace TrafficControlService.Proxies
{
    public class GovernmentService
    {
        private HttpClient _httpClient;
        private JsonSerializerOptions _serializerOptions;

        public GovernmentService(HttpClient httpClient)
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
                $"rdw/vehicle/{licenseNumber}", _serializerOptions);
        }
    }
}