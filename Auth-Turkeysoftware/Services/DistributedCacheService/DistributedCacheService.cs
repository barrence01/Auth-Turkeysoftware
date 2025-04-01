using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Auth_Turkeysoftware.Services.DistributedCacheService
{
    public class DistributedCacheService : IDistributedCacheService
    {
        private readonly IPostgresCacheRepository _cacheService;
        public DistributedCacheService(IPostgresCacheRepository postgresCacheRepository) { 
            _cacheService = postgresCacheRepository;
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, object value, TimeSpan expiration)
        {
            if (string.IsNullOrWhiteSpace(key)) {
                throw new ArgumentException("A chave não pode ser nula ou vazia.", nameof(key));
            }
            if (key.Length > 255) {
                throw new ArgumentException("A chave não pode ter mais de 255 caracteres.", nameof(key));
            }

            await _cacheService.SetAsync(key, value, expiration);
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key)
        {
            return await _cacheService.GetAsync<T>(key);
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key)
        {
            await _cacheService.RemoveAsync(key);
        }
    }
}
