namespace Auth_Turkeysoftware.Services.ExternalServices
{
    public sealed class HttpClientSingleton
    {
        private static readonly HttpClient _httpClient;

        static HttpClientSingleton()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(1)
            };
        }

        public static async Task<string> GetAsync(string url)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                return "Request timed out.";
            }
            catch (HttpRequestException ex)
            {
                return $"Request error: {ex.Message}";
            }
        }
    }
}
