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
            var cacheEntry = CreateCacheEntry(key, value, expiration);

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
            if (cacheEntry == null || cacheEntry.ExpiresAtTime <= DateTimeOffset.UtcNow)
            {
                await RemoveAsync(key);
                return default;
            }

            return DeserializeCacheEntry<T>(cacheEntry.Value);
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

        private CacheEntryModel CreateCacheEntry(string key, object value, TimeSpan expiration)
        {
            var jsonData = JsonSerializer.Serialize(value);
            var encodedData = Encoding.UTF8.GetBytes(jsonData);

            return new CacheEntryModel
            {
                Id = key,
                Value = encodedData,
                ExpiresAtTime = DateTimeOffset.UtcNow.Add(expiration),
                AbsoluteExpiration = DateTimeOffset.UtcNow.Add(expiration)
            };
        }

        private async Task UpdateCachedEntry(CacheEntryModel cacheEntry)
        {
            await _dbContext.DistributedCache
                            .Where(p => p.Id == cacheEntry.Id)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(p => p.Value, cacheEntry.Value)
                                .SetProperty(p => p.ExpiresAtTime, cacheEntry.ExpiresAtTime)
                                .SetProperty(p => p.SlidingExpiration, cacheEntry.SlidingExpiration)
                                .SetProperty(p => p.AbsoluteExpiration, cacheEntry.AbsoluteExpiration)
                            );
        }

        private T? DeserializeCacheEntry<T>(byte[] value)
        {
            var jsonData = Encoding.UTF8.GetString(value);
            return JsonSerializer.Deserialize<T>(jsonData);
        }
    }
}