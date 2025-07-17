namespace Auth_Turkeysoftware.Infraestructure.HttpClients
{
    public sealed class HttpClientSingleton
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientSingleton> _logger;

        // Inject ILogger via constructor
        public HttpClientSingleton(ILogger<HttpClientSingleton> logger)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            _logger = logger;
        }

        public async Task<string?> GetAsync(string url)
        {
            try
            {
                _logger.LogInformation("Requisição Externa. Executando método GET: {Url}", url);
                using var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogInformation(ex, "A requisição ultrapassou o limite de tempo.: {Url}", url);
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ocorreu um erro durante a requisição: {Url}", url);
                return null;
            }
        }

        public async Task<string?> GetAsync(string url, TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    _logger.LogInformation("Requisição Externa. Executando método GET: {Url}", url);
                    using var response = await _httpClient.GetAsync(url, cts.Token);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogInformation(ex, "A requisição ultrapassou o limite de tempo.: {Url}", url);
                    return null;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Ocorreu um erro durante a requisição: {Url}", url);
                    return null;
                }
            }
        }
    }
}
