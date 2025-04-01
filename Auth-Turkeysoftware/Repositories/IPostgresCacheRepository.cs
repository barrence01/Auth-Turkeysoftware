using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Repositories
{
    /// <summary>
    /// Interface para um repositório de cache utilizando o banco de dados PostgreSQL.
    /// Esta é uma versão customizada de um DistributedCache.
    /// </summary>
    public interface IPostgresCacheRepository
    {
        /// <summary>
        /// Armazena um valor no cache com uma chave específica e um tempo de expiração.
        /// </summary>
        /// <param name="key">A chave única para o valor armazenado.</param>
        /// <param name="value">O valor a ser armazenado no cache.</param>
        /// <param name="expiration">O tempo de expiração do cache.</param>
        Task SetAsync(string key, object value, TimeSpan expiration);

        /// <summary>
        /// Armazena um valor no cache com uma chave específica e um tempo de expiração.
        /// </summary>
        /// <param name="key">A chave única para o valor armazenado.</param>
        /// <param name="value">O valor a ser armazenado no cache.</param>
        /// <param name="expiration">O tempo de expiração do cache.</param>
        /// <param name="options">Opções extras para extensão do prazo de expiração do cache.</param>
        Task SetAsync(string key, object value, TimeSpan expiration, CacheEntryOptions options);

        /// <summary>
        /// Recupera um valor do cache com base na chave fornecida.
        /// </summary>
        /// <typeparam name="T">O tipo do valor a ser recuperado.</typeparam>
        /// <param name="key">A chave única do valor armazenado.</param>
        /// <returns>O valor armazenado no cache ou null se a chave não existir.</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Remove um valor do cache com base na chave fornecida.
        /// </summary>
        /// <param name="key">A chave única do valor a ser removido.</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// Obtém uma lista de entradas de cache que correspondem a um campo JSON específico.
        /// </summary>
        /// <param name="idPattern">Padrão de ID para filtrar as entradas de cache.</param>
        /// <param name="fieldName">Nome do campo JSON a ser filtrado.</param>
        /// <param name="fieldValue">Valor do campo JSON a ser filtrado.</param>
        /// <param name="useLikeId">Indica se deve usar LIKE para o padrão de ID.</param>
        /// <returns>Uma lista de entradas de cache que correspondem aos critérios fornecidos.</returns>
        Task<List<CacheEntryModel>> GetEntitiesByJsonField(string idPattern, string fieldName, string fieldValue, bool useLikeId);
    }
}
