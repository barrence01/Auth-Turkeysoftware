using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Services.DistributedCacheService
{
    public class DistributedCacheService : IDistributedCacheService
    {
        private readonly IPostgresCacheRepository _cacheRepository;
        public DistributedCacheService(IPostgresCacheRepository postgresCacheRepository) { 
            _cacheRepository = postgresCacheRepository;
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

            await _cacheRepository.SetAsync(key, value, expiration);
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key)) {
                throw new ArgumentException("A chave não pode ser nula ou vazia.", nameof(key));
            }
            if (key.Length > 255) {
                throw new ArgumentException("A chave não pode ter mais de 255 caracteres.", nameof(key));
            }

            await _cacheRepository.SetAsync(key, value, true);
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, object value, TimeSpan expiration, CacheEntryOptions options)
        {
            if (string.IsNullOrWhiteSpace(key)) {
                throw new ArgumentException("A chave não pode ser nula ou vazia.", nameof(key));
            }
            if (key.Length > 255) {
                throw new ArgumentException("A chave não pode ter mais de 255 caracteres.", nameof(key));
            }
            if (options.AbsoluteExpiration.HasValue && (options.AbsoluteExpiration <= DateTimeOffset.UtcNow.Add(expiration))) {
                throw new ArgumentException("Data de expiração final(AbsoluteExpiration) não pode ser menor que a data de expiração(expiration).", nameof(key));
            }

            await _cacheRepository.SetAsync(key, value, expiration, options);
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key)
        {
            return await _cacheRepository.GetAsync<T>(key);
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key)
        {
            await _cacheRepository.RemoveAsync(key);
        }

        /// <inheritdoc/>
        public async Task<bool> IsCachedAsync(string key)
        {
            return await _cacheRepository.IsCachedAsync(key);
        }
    }
}
