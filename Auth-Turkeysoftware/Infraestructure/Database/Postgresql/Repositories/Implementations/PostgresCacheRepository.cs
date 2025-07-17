using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Interfaces;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Implementations
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

            if (!await IsAlreadyInserted(key))
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

            if (!await IsAlreadyInserted(key))
            {
                await _dbContext.DistributedCache.AddAsync(cacheEntry);
                await _dbContext.SaveChangesAsync();
                return;
            }

            await UpdateCachedEntry(cacheEntry, keepExpTime);
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key, object value, TimeSpan expiration, CacheEntryOptions options)
        {
            var cacheEntry = CreateCacheEntry(key, value, expiration, options);

            if (!await IsAlreadyInserted(key))
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

            return await ValidateDeserializeCachedElement<T>(cacheEntry);
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key)
        {
            await _dbContext.DistributedCache
                            .Where(p => p.Id == key)
                            .ExecuteDeleteAsync();
        }

        public async Task<bool> IsAlreadyInserted(string key)
        {
            return await _dbContext.DistributedCache.AsNoTracking().AnyAsync(e => e.Id == key);
        }

        /// <inheritdoc/>
        public async Task<bool> IsCachedAsync(string key)
        {
            CacheEntryModel? cacheEntry = await _dbContext.DistributedCache
                                                          .AsNoTracking()
                                                          .Where(e => e.Id == key)
                                                          .Select(s => new CacheEntryModel
                                                          {
                                                              Id = s.Id,
                                                              ExpiresAtTime = s.ExpiresAtTime,
                                                              AbsoluteExpiration = s.AbsoluteExpiration,
                                                              SlidingExpiration = s.SlidingExpiration
                                                          })
                                                          .Take(1)
                                                          .FirstOrDefaultAsync();

            if (await ValidateCachedElement(cacheEntry) == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Cria uma nova entrada de cache com a chave, valor, tempo de expiração e opções especificadas.
        /// </summary>
        /// <param name="key">Chave única que identifica a entrada no cache.</param>
        /// <param name="value">Objeto a ser armazenado no cache (será serializado para JSON).</param>
        /// <param name="expiration">Tempo de vida padrão para a entrada de cache.</param>
        /// <param name="options">Opções adicionais de cache como expiração absoluta ou deslizante.</param>
        /// <returns>Um novo <see cref="CacheEntryModel"/> configurado com os parâmetros fornecidos.</returns>
        private static CacheEntryModel CreateCacheEntry(string key, object value, TimeSpan expiration, CacheEntryOptions? options)
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

        /// <summary>
        /// Atualiza uma entrada existente no cache, opcionalmente mantendo seu tempo de expiração.
        /// </summary>
        /// <param name="cacheEntry">Modelo da entrada de cache com os valores atualizados.</param>
        /// <param name="keepExpTime">Se true, mantém o tempo de expiração existente; caso contrário atualiza todas as propriedades.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona de atualização.</returns>
        private async Task UpdateCachedEntry(CacheEntryModel cacheEntry, bool keepExpTime = false)
        {
            if (keepExpTime)
            {
                await _dbContext.DistributedCache
                .Where(p => p.Id == cacheEntry.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Value, cacheEntry.Value)
                );

            }
            else
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
        }

        /// <summary>
        /// Valida um elemento em cache, verificando expiração e aplica expiração deslizante.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto armazenado em cache.</typeparam>
        /// <param name="cacheEntry">Entrada de cache a ser validada, ou null se não encontrada.</param>
        /// <returns>
        /// O valor desserializado se válido; caso contrário remove a entrada expirada e retorna default(T).
        /// </returns>
        private async Task<T?> ValidateDeserializeCachedElement<T>(CacheEntryModel? cacheEntry)
        {

            DateTimeOffset currentTime = DateTimeOffset.UtcNow;

            if (cacheEntry == null) { return default; }

            if (cacheEntry.AbsoluteExpiration <= currentTime || !cacheEntry.SlidingExpiration.HasValue && cacheEntry.ExpiresAtTime <= currentTime)
            {
                return default;
            }

            if (cacheEntry.SlidingExpiration.HasValue && cacheEntry.AbsoluteExpiration <= currentTime)
            {
                cacheEntry.ExpiresAtTime = currentTime.Add(cacheEntry.SlidingExpiration.Value);
                await _dbContext.SaveChangesAsync();
            }

            return DeserializeCacheEntry<T>(cacheEntry.Value);
        }

        /// <summary>
        /// Valida um elemento em cache, verificando expiração e aplica expiração deslizante.
        /// </summary>
        /// <param name="cacheEntry">Entrada de cache a ser validada, ou null se não encontrada.</param>
        /// <returns>
        /// Retorna o cacheEntry se não estiver expirado.
        /// </returns>
        private async Task<CacheEntryModel?> ValidateCachedElement(CacheEntryModel? cacheEntry)
        {
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;

            if (cacheEntry == null) { return default; }

            if (cacheEntry.AbsoluteExpiration <= currentTime || !cacheEntry.SlidingExpiration.HasValue && cacheEntry.ExpiresAtTime <= currentTime)
            {
                return default;
            }

            if (cacheEntry.SlidingExpiration.HasValue && cacheEntry.AbsoluteExpiration <= currentTime)
            {
                cacheEntry.ExpiresAtTime = currentTime.Add(cacheEntry.SlidingExpiration.Value);
                await _dbContext.SaveChangesAsync();
            }

            return cacheEntry;
        }

        /// <summary>
        /// Desserializa um array de bytes em cache para um objeto do tipo T.
        /// </summary>
        /// <typeparam name="T">Tipo alvo para desserialização.</typeparam>
        /// <param name="value">Array de bytes contendo os dados JSON serializados.</param>
        /// <returns>O objeto desserializado ou null se a desserialização falhar.</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando os dados JSON não podem ser desserializados no tipo alvo.
        /// </exception>
        private static T? DeserializeCacheEntry<T>(byte[] value)
        {
            try
            {
                var jsonData = Encoding.UTF8.GetString(value);
                return JsonSerializer.Deserialize<T>(jsonData);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Houve uma falha na tentativa de desserializar o JSON", ex);
            }
        }
    }
}