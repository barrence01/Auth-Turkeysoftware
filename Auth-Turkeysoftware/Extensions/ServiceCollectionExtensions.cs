using Auth_Turkeysoftware.Configurations.Models;
using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Services.DistributedCacheService;
using Auth_Turkeysoftware.Services.ExternalServices;
using Auth_Turkeysoftware.Services;
using Auth_Turkeysoftware.Test.Repositories;
using Auth_Turkeysoftware.Services.MailService;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Controllers.Handlers;

namespace Auth_Turkeysoftware.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IPostgresCacheRepository, PostgresCacheRepository>();
            services.AddScoped<IDistributedCacheService, DistributedCacheService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITwoFactorRepository, TwoFactorRepository>();
            services.AddScoped<IUserSessionService, UserSessionService>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            services.AddScoped<IExternalApiService, ExternalApiService>();
            services.AddScoped<IAdministrationService, AdministrationService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAccountRecoveryService, AccountRecoveryService>();
            services.AddScoped<IRegisterUserService, RegisterUserService>();
            services.AddScoped<ICommunicationService, CommunicationService>();
            services.AddScoped<ITestDataRepository, TestDataRepository>();

            // Filters
            services.AddScoped<LoginFilter>();
            services.AddScoped<AdminActionLoggingFilterAsync>();

            return services;
        }

        public static IServiceCollection AddCustomSingletons(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<JwtSettingsSingleton>(GetJwtSettingsSingleton());
            services.AddSingleton<HttpClientSingleton>();
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            services.AddSingleton<EmailSettingsSingleton>();
            services.Configure<EmailTokenProviderSettings>(config.GetSection("EmailTokenProviderSettings"));
            services.AddSingleton<EmailTokenProviderSingleton>();

            return services;
        }

        public static IServiceCollection AddCustomTransients(this IServiceCollection services, IConfiguration config)
        {
            // Mail Service
            services.AddTransient<IEmailService, EmailService>();

            return services;
        }

        public static IServiceCollection AddCustomHandlers(this IServiceCollection services, IConfiguration config)
        {
            // Global Exeption Handler
            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        public static JwtSettingsSingleton GetJwtSettingsSingleton() {
            return new JwtSettingsSingleton(new JwtSettings
            {
                EncryptionKey = GetRequiredEnvVar("JWT_ENCRYPTION_KEY"),
                LoginSecretKey = GetRequiredEnvVar("JWT_LOGIN_SECRET"),
                AccessSecretKey = GetRequiredEnvVar("JWT_ACCESS_SECRET"),
                RefreshSecretKey = GetRequiredEnvVar("JWT_REFRESH_SECRET"),
                Issuer = GetRequiredEnvVar("JWT_ISSUER"),
                Audience = GetRequiredEnvVar("JWT_AUDIENCE"),
                Domain = GetRequiredEnvVar("JWT_DOMAIN"),
                AccessTokenValidityInMinutes = GetEnvVarAsInt("JWT_ACCESS_TOKEN_MINUTES"),
                RefreshTokenValidityInMinutes = GetEnvVarAsInt("JWT_REFRESH_TOKEN_MINUTES"),
                RefreshTokenPath = GetRequiredEnvVar("JWT_REFRESH_TOKEN_PATH") ?? "/api/Auth/refresh-token",
                AccessTokenPath = GetRequiredEnvVar("JWT_ACCESS_TOKEN_PATH") ?? "/"
            });
        }

        private static string GetRequiredEnvVar(string name) =>
            Environment.GetEnvironmentVariable(name) ??
            throw new InvalidOperationException($"Environment variable '{name}' is required.");

        private static int GetEnvVarAsInt(string name) =>
            int.TryParse(Environment.GetEnvironmentVariable(name), out var result) ? result
            : throw new InvalidOperationException($"Environment variable '{name}' must be an integer.");
    }
}
