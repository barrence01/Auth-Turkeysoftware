using Serilog;

namespace Auth_Turkeysoftware.Services.ExternalServices
{
    public sealed class HttpClientSingleton
    {
        private static readonly HttpClient _httpClient;

        static HttpClientSingleton()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public static async Task<string?> GetAsync(string url)
        {
            try
            {
                Log.Information($"Requisição Externa. Executando método GET: {url}");
                using var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                Log.Information($"A requisição ultrapassou o limite de tempo.: {url}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                Log.Error($"Ocorreu um erro durante a requisição: {url}");
                Log.Error(ex.Message);
                return null;
            }
        }

        public static async Task<string?> GetAsync(string url, TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    Log.Information($"Requisição Externa. Executando método GET: {url}");
                    using var response = await _httpClient.GetAsync(url, cts.Token);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (TaskCanceledException)
                {
                    Log.Information($"A requisição ultrapassou o limite de tempo.: {url}");
                    return null;
                }
                catch (HttpRequestException ex)
                {
                    Log.Error($"Ocorreu um erro durante a requisição: {url}");
                    Log.Error(ex.Message);
                    return null;
                }
            }
        }
    }
}
