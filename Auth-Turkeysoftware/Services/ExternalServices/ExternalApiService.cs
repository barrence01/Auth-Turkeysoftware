using Auth_Turkeysoftware.Models;
using Serilog;
using System.Text.Json;

namespace Auth_Turkeysoftware.Services.ExternalServices
{
    public class ExternalApiService : IExternalApiService
    {
        public async Task<IpDetailsModel?> GetIpDetails(string address)
        {
            Log.Information("Executando método GetIpDetails :: ExternalApiService");
            Log.Information($"Endereço de IP: {address}");
            try
            {
                string url = string.Concat(@"http://ip-api.com/json/", address);
                string json = await HttpClientSingleton.GetAsync(url);
                Log.Information($"Response: {json}");
                return JsonSerializer.Deserialize<IpDetailsModel>(json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Não foi possível obter os detalhes do endereço de IP.");
                return null;
            }
        }
    }
}
