using Auth_Turkeysoftware.Domain.Models.VOs;
using Auth_Turkeysoftware.Infraestructure.HttpClients;
using System.Text.Json;

namespace Auth_Turkeysoftware.Infraestructure.External.Services
{
    public class GeoLocationService : IGeoLocationService
    {
        private readonly ILogger<GeoLocationService> _logger;

        private readonly HttpClientSingleton _httpClientSingleton;

        public GeoLocationService(ILogger<GeoLocationService> logger, HttpClientSingleton httpClientSingleton)
        {
            _logger = logger;
            _httpClientSingleton = httpClientSingleton;
        }

        public async Task<IpDetailsVO?> GetIpDetails(string ipAddress)
        {
            _logger.LogInformation("Executando método GetIpDetails :: ExternalApiService");
            try
            {
                string url = string.Concat(@"http://ip-api.com/json/", ipAddress);
                string? json = await _httpClientSingleton.GetAsync(url, TimeSpan.FromSeconds(0.5));
                if (string.IsNullOrEmpty(json))
                    return null;

                _logger.LogInformation("Response: {Json}", json);
                return JsonSerializer.Deserialize<IpDetailsVO>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Não foi possível converter o JSON para o objeto IpDetailsModel.");
                return null;
            }
        }
    }
}
