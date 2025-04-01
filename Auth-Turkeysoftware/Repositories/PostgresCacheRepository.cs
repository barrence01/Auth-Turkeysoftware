using Auth_Turkeysoftware.Repositories.Context;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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

        /// <inheritdoc/>
        public async Task<List<CacheEntryModel>> GetEntitiesByJsonField(string idPattern, string fieldName, string fieldValue, bool useLikeId)
        {
            if (idPattern.Contains("%"))
                idPattern = idPattern.Replace("%", "");

            string query = @"
                SELECT d.""Id"", d.""Value"", d.""AbsoluteExpiration"", d.""ExpiresAtTime"", d.""SlidingExpiration""
                FROM auth.""DistributedCache"" as d
                WHERE convert_from(d.""Value"", 'UTF-8')::jsonb ->> @fieldName = @fieldValue
            ";

            if (useLikeId)
            {
                idPattern += "%";
                query += @" AND d.""Id"" LIKE @idPattern";
            }
            else
            {
                query += @" AND d.""Id"" = @idPattern";
            }

            var parameters = new[]
            {
                new NpgsqlParameter("@idPattern", idPattern),
                new NpgsqlParameter("@fieldName", fieldName),
                new NpgsqlParameter("@fieldValue", fieldValue)
            };

            return await _dbContext.DistributedCache.FromSqlRaw(query, parameters).ToListAsync();
        }
    }
}