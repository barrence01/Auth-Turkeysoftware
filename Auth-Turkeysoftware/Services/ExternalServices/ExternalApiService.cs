using Auth_Turkeysoftware.Models.DTOs;
using System.Text.Json;

namespace Auth_Turkeysoftware.Services.ExternalServices
{
    public class ExternalApiService : IExternalApiService
    {
        private readonly ILogger<ExternalApiService> _logger;

        private readonly HttpClientSingleton _httpClientSingleton;

        public ExternalApiService(ILogger<ExternalApiService> logger, HttpClientSingleton httpClientSingleton) { 
            _logger = logger;
            _httpClientSingleton = httpClientSingleton;
        }

        public async Task<IpDetailsDto?> GetIpDetails(string ipAddress)
        {
            _logger.LogInformation("Executando método GetIpDetails :: ExternalApiService");
            try
            {
                string url = string.Concat(@"http://ip-api.com/json/", ipAddress);
                string? json = await _httpClientSingleton.GetAsync(url, TimeSpan.FromSeconds(0.5));
                if (string.IsNullOrEmpty(json))
                    return null;

                _logger.LogInformation("Response: {Json}", json);
                return JsonSerializer.Deserialize<IpDetailsDto>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Não foi possível converter o JSON para o objeto IpDetailsModel.");
                return null;
            }
        }
    }
}
