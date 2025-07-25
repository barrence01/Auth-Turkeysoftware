﻿using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Interfaces
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
        /// Armazena um valor no cache com uma chave específica.<br/>
        /// Se o objeto em cache não estiver expirado, o atualiza.
        /// Caso contrário, um objeto de tempo de expiração zerado será criado.
        /// </summary>
        /// <param name="key">A chave única para o valor armazenado.</param>
        /// <param name="value">O valor a ser armazenado no cache.</param>
        /// <param name="keepExpTime">Flag que indica se deve ser mantido os valores originais para expiração do objeto armazenado.</param>
        Task SetAsync(string key, object value, bool keepExpTime);

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
        /// Verifica se uma entrada existe no cache distribuído com a chave especificada.<br/>
        /// </summary>
        /// <param name="key">A chave única do item de cache a ser verificado.</param>
        /// <returns>
        /// Uma tarefa que representa a operação assíncrona, contendo true se a chave existir no cache,
        /// ou false caso contrário.
        /// </returns>
        Task<bool> IsCachedAsync(string key);
    }
}
