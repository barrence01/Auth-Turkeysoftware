using Auth_Turkeysoftware.Repositories.Context;
using Laraue.EfCoreTriggers.PostgreSql.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Extensions
{
    public static class DbContextExtensions
    {
        public static IServiceCollection AddAppDbContexts(this IServiceCollection services, IConfiguration config)
        {
            var connStringDefault = config.GetConnectionString("DefaultConnection");
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
            var connStringSecondary = config.GetConnectionString("CacheDbConnection");
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
