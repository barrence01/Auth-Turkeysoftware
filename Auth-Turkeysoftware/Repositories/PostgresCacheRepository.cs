using Auth_Turkeysoftware.Repositories.Context;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Auth_Turkeysoftware.Repositories
{
    public class PostgresCacheRepository : IPostgresCacheRepository
    {
        private readonly CacheDbContext _dbContext;

        public PostgresCacheRepository(CacheDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, object value, TimeSpan expiration)
        {
            var cacheEntry = CreateCacheEntry(key, value, expiration, default);

            if (!await IsCached(key))
            {
                await _dbContext.DistributedCache.AddAsync(cacheEntry);
                await _dbContext.SaveChangesAsync();
                return;
            }

            await UpdateCachedEntry(cacheEntry);
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, object value, bool keepExpTime)
        {
            var cacheEntry = CreateCacheEntry(key, value, default, default);

            if (!await IsCached(key))
            {
                await _dbContext.DistributedCache.AddAsync(cacheEntry);
                await _dbContext.SaveChangesAsync();
                return;
            }

            await UpdateCachedEntry(cacheEntry, keepExpTime);
        }

        public async Task SetAsync(string key, object value, TimeSpan expiration, CacheEntryOptions options)
        {
            var cacheEntry = CreateCacheEntry(key, value, expiration, options);

            if (!await IsCached(key))
            {
                await _dbContext.DistributedCache.AddAsync(cacheEntry);
                await _dbContext.SaveChangesAsync();
                return;
            }

            await UpdateCachedEntry(cacheEntry);
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T>(string key)
        {
            var cacheEntry = await _dbContext.DistributedCache.FindAsync(key);

            return await ValidateCachedElement<T>(key, cacheEntry);
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key)
        {
            await _dbContext.DistributedCache
                  .Where(p => p.Id == key)
                  .ExecuteDeleteAsync();
        }

        private async Task<bool> IsCached(string key)
        {
            return await _dbContext.DistributedCache
                                   .AsNoTracking()
                                   .AnyAsync(p => p.Id == key);
        }

        private CacheEntryModel CreateCacheEntry(string key, object value, TimeSpan expiration, CacheEntryOptions? options)
        {
            var jsonData = JsonSerializer.Serialize(value);
            var encodedData = Encoding.UTF8.GetBytes(jsonData);

            var entry = new CacheEntryModel
                        {
                            Id = key,
                            Value = encodedData,
                            ExpiresAtTime = DateTimeOffset.UtcNow.Add(expiration),
                            AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expiration)
                        };

            if (options != null)
            {
                entry.SlidingExpiration = options.SlidingExpiration;
                entry.AbsoluteExpiration = options.AbsoluteExpiration;
            }
            return entry;
        }

        private async Task UpdateCachedEntry(CacheEntryModel cacheEntry, bool keepExpTime = false)
        {
            if (keepExpTime) {
                await _dbContext.DistributedCache
                .Where(p => p.Id == cacheEntry.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Value, cacheEntry.Value)
                );

            } else {
                await _dbContext.DistributedCache
                .Where(p => p.Id == cacheEntry.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Value, cacheEntry.Value)
                    .SetProperty(p => p.ExpiresAtTime, cacheEntry.ExpiresAtTime)
                    .SetProperty(p => p.SlidingExpiration, cacheEntry.SlidingExpiration)
                    .SetProperty(p => p.AbsoluteExpiration, cacheEntry.AbsoluteExpiration)
                );
            }
        }

        private async Task<T?> ValidateCachedElement<T>(string key, CacheEntryModel? cacheEntry)
        {

            DateTimeOffset currentTime = DateTimeOffset.UtcNow;

            if (cacheEntry == null || (cacheEntry.AbsoluteExpiration <= currentTime || cacheEntry.ExpiresAtTime <= currentTime))
            {
                await RemoveAsync(key);
                return default;
            }

            if (cacheEntry.AbsoluteExpiration <= currentTime && cacheEntry.SlidingExpiration.HasValue)
            {
                cacheEntry.ExpiresAtTime = currentTime.Add(cacheEntry.SlidingExpiration.Value);
                await _dbContext.SaveChangesAsync();
            }

            return DeserializeCacheEntry<T>(cacheEntry.Value);
        }

        private T? DeserializeCacheEntry<T>(byte[] value)
        {
            try
            {
                var jsonData = Encoding.UTF8.GetString(value);
                return JsonSerializer.Deserialize<T>(jsonData);
            } catch (JsonException ex)  {
                throw new InvalidOperationException("Houve uma falha na tentativa de desserializar o JSON", ex);
            }
        }
    }
}