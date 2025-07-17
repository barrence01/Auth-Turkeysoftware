using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.DbContext;
using Auth_Turkeysoftware.Shared.Utils;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Shared.Extensions
{
    /// <summary>
    /// Extensões para configuração de DbContexts da aplicação
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Configura e registra os DbContexts principais da aplicação
        /// </summary>
        /// <param name="services">Coleção de serviços DI</param>
        /// <param name="config">Configuração da aplicação</param>
        /// <returns>IServiceCollection para encadeamento</returns>
        /// <remarks>
        /// <para>Configura dois DbContexts com pooling de conexões:</para>
        /// <list type="number">
        /// <item>
        /// <term>AppDbContext</term>
        /// <description>
        /// Contexto principal com:
        /// - Conexão padrão (DefaultConnection)
        /// - Resiliência a falhas (retry automático)
        /// - Suporte a triggers PostgreSQL
        /// - Tratamento especial para deadlocks e timeouts
        /// </description>
        /// </item>
        /// <item>
        /// <term>CacheDbContext</term>
        /// <description>
        /// Contexto secundário para operações realizadas fora do escopo da requisição atual(stateless):
        /// - Conexão separada (SecondaryConnection)
        /// - Resiliência básica a falhas
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// Os códigos de erro tratados automaticamente incluem:
        /// 57014 (Timeout), 40P01 (Deadlock), e outros erros de conexão
        /// </para>
        /// </remarks>
        public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration config)
        {
            var connStringDefault = ConfigUtils.GetRequiredEnvVar("AUTH_DB_CONN");
            services.AddDbContextPool<AppDbContext>(options =>
            {
                options.UseNpgsql(connStringDefault, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure();
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: new List<string>
                    {
                        "57014", // Query timeout (PostgreSQL)
                        "53300", // Too many connections
                        "53400", // Configuration limit exceeded
                        "08000", // Connection exception
                        "08003", // Connection does not exist
                        "08006", // Connection failure
                        "08001", // SQL-client unable to establish SQL-connection
                        "08004", // SQL-server rejected establishment of SQL-connection
                        "08007", // Transaction resolution unknown
                        "40P01"  // Deadlock detected
                    });
                });
                options.UsePostgreSqlTriggers();
            });

            ////
            // Acesso separado para o dbcontext utilizado para operações que não dependam de estado
            ////
            var connStringSecondary = ConfigUtils.GetRequiredEnvVar("AUTH_SECONDARY_DB_CONN");
            services.AddDbContextPool<CacheDbContext>(options =>
            {
                options.UseNpgsql(connStringSecondary, npgsql =>
                {
                    npgsql.EnableRetryOnFailure();
                });
            });

            return services;
        }
    }
}
