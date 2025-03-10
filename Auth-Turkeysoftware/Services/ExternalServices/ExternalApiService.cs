using Auth_Turkeysoftware.Models;
using Serilog;
using System;
using System.Text.Json;

namespace Auth_Turkeysoftware.Services.ExternalServices
{
    public class ExternalApiService : IExternalApiService
    {
        public async Task<IpDetailsModel?> GetIpDetails(string address)
        {
            Log.Information("Executando método GetIpDetails :: ExternalApiService");
            try
            {
                string url = string.Concat(@"http://ip-api.com/json/", address);
                string? json = await HttpClientSingleton.GetAsync(url, TimeSpan.FromSeconds(0.5));
                if (string.IsNullOrEmpty(json))
                    return null;

                Log.Information($"Response: {json}");
                return JsonSerializer.Deserialize<IpDetailsModel>(json);
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Não foi possível converter o JSON para o objeto IpDetailsModel.");
                return null;
            }
        }
    }
}
