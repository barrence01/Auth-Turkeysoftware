using Auth_Turkeysoftware.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Helpers
{
    /// <summary>
    /// Classe utilitária para execução de operações dentro de transações no banco de dados.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Fornece métodos de extensão para <see cref="AppDbContext"/> que garantem a execução de operações
    /// dentro de transações com tratamento adequado de commit e rollback.
    /// </para>
    /// <para>
    /// Implementa o padrão Execution Strategy do Entity Framework Core para lidar com falhas transitórias.
    /// </para>
    /// </remarks>
    public static class TransactionHelper
    {
        /// <summary>
        /// Executa a operação assíncrona recebida dentro de uma transação de banco de dados.
        /// </summary>
        /// <typeparam name="T">Tipo do resultado da operação.</typeparam>
        /// <param name="dbContext">Contexto do banco de dados.</param>
        /// <param name="operation">Operação assíncrona a ser executada na transação.</param>
        /// <returns>Task contendo o resultado da operação.</returns>
        /// <exception cref="ArgumentNullException">Lançada quando dbContext ou operation são nulos.</exception>
        /// <exception cref="Exception">Qualquer exceção lançada pela operation será repassada após rollback.</exception>
        /// <remarks>
        /// <para>
        /// Este método:<br/>
        /// 1. Cria uma estratégia de execução para lidar com falhas transitórias<br/>
        /// 2. Inicia uma nova transação<br/>
        /// 3. Executa a operação<br/>
        /// 4. Faz commit se bem-sucedido ou rollback em caso de erro<br/><br/>
        /// </para>
        /// <example>
        /// <code>
        /// var result = await dbContext.ExecuteWithTransactionAsync(async () => 
        /// {
        ///     // Operações no banco de dados
        ///     return await service.ProcessAsync();
        /// });
        /// </code>
        /// </example>
        /// </remarks>
        public static async Task<T> ExecuteWithTransactionAsync<T>(
            this AppDbContext dbContext,
            Func<Task<T>> operation)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    var result = await operation();
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        /// <summary>
        /// Executa uma operação síncrona dentro de uma transação de banco de dados.
        /// </summary>
        /// <typeparam name="T">Tipo do resultado da operação.</typeparam>
        /// <param name="dbContext">Contexto do banco de dados.</param>
        /// <param name="operation">Operação síncrona a ser executada na transação.</param>
        /// <returns>Resultado da operação.</returns>
        /// <exception cref="ArgumentNullException">Lançada quando dbContext ou operation são nulos.</exception>
        /// <exception cref="Exception">Qualquer exceção lançada pela operation será repassada após rollback.</exception>
        /// <remarks>
        /// <para>
        /// Versão síncrona do método ExecuteWithTransactionAsync, com o mesmo comportamento
        /// de gerenciamento de transações, mas para operações não-assíncronas.<br/>
        /// <b>
        /// Recomendado utilizar métodos async para operações em banco de dados.
        /// </b>
        /// </para>
        /// <example>
        /// <code>
        /// var result = dbContext.ExecuteWithTransaction(() => 
        /// {
        ///     // Operações no banco de dados
        ///     return service.Process();
        /// });
        /// </code>
        /// </example>
        /// </remarks>
        public static T ExecuteWithTransaction<T>(
            this AppDbContext dbContext,
            Func<T> operation)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.Execute(() =>
            {
                using var transaction = dbContext.Database.BeginTransaction();
                try
                {
                    var result = operation();
                    transaction.Commit();
                    return result;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }
    }
}
