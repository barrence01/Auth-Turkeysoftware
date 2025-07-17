using Auth_Turkeysoftware.Test.Repositories;
using Auth_Turkeysoftware.Infraestructure.Configurations.Models;
using Auth_Turkeysoftware.Infraestructure.Configurations.Singletons;
using Auth_Turkeysoftware.Infraestructure.DistributedCache;
using Auth_Turkeysoftware.Infraestructure.HttpClients;
using Auth_Turkeysoftware.Infraestructure.Mail;
using Auth_Turkeysoftware.Infraestructure.External.Services;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Interfaces;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Implementations;
using Auth_Turkeysoftware.API.Filters;
using Auth_Turkeysoftware.API.Handlers;
using Auth_Turkeysoftware.Shared.Utils;
using Auth_Turkeysoftware.Domain.Services.Interfaces;
using Auth_Turkeysoftware.Domain.Services.Implementations;

namespace Auth_Turkeysoftware.Shared.Extensions
{
    /// <summary>
    /// Classe de extensão para configuração de injeção de dependência
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra serviços com ciclo de vida Scoped (uma instância por requisição)
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <returns>IServiceCollection para encadeamento</returns>
        /// <remarks>
        /// Registra:
        /// <list type="termdef">
        /// <item>- Repositórios (PostgresCache, TwoFactor, UserSession, etc.)</item>
        /// <item>- Serviços de domínio (User, Authentication, AccountRecovery, etc.)</item>
        /// <item>- Serviços de infraestrutura (Cache, ExternalApi, Communication etc.)</item>
        /// <item>- Filtros customizados (Login, AdminActionLogging, etc.)</item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddCustomScoped(this IServiceCollection services)
        {
            services.AddScoped<IPostgresCacheRepository, PostgresCacheRepository>();
            services.AddScoped<IDistributedCache, DistributedCache>();
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

        /// <summary>
        /// Registra serviços com ciclo de vida Singleton (uma instância por aplicação)
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <param name="config">Configuração da aplicação</param>
        /// <returns>IServiceCollection para encadeamento</returns>
        /// <remarks>
        /// Configura:
        /// <list type="termdef">
        /// <item>- JwtSettings (configurações de token JWT)</item>
        /// <item>- HttpClientSingleton (cliente HTTP compartilhado)</item>
        /// <item>- Configurações de e-mail (EmailSettings, EmailTokenProvider)</item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddCustomSingletons(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton(GetJwtSettingsSingleton(config));
            services.AddSingleton<HttpClientSingleton>();
            services.AddSingleton(GetAuthEmailSettingsSingleton(config));
            services.Configure<EmailTokenProviderSettings>(config.GetSection("EmailTokenProviderSettings"));
            services.AddSingleton<EmailTokenProviderSingleton>();

            return services;
        }

        /// <summary>
        /// Registra serviços com ciclo de vida Transient (nova instância a cada uso)
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <param name="config">Configuração da aplicação</param>
        /// <returns>IServiceCollection para encadeamento</returns>
        public static IServiceCollection AddCustomTransients(this IServiceCollection services, IConfiguration config)
        {
            // Mail Service
            services.AddTransient<IEmailService, EmailService>();

            return services;
        }

        /// <summary>
        /// Configura handlers globais para tratamento de exceções
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <param name="config">Configuração da aplicação</param>
        /// <returns>IServiceCollection para encadeamento</returns>
        public static IServiceCollection AddCustomHandlers(this IServiceCollection services, IConfiguration config)
        {
            // Global Exeption Handler
            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        /// <summary>
        /// Cria e configura a instância singleton para JWT
        /// </summary>
        /// <returns>Instância configurada de JwtSettingsSingleton</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando variáveis de ambiente necessárias não estão configuradas
        /// </exception>
        /// <remarks>
        /// Obtém todas as configurações necessárias de variáveis de ambiente para configuração do JWT.
        /// <list type="termdef">
        /// <item>- Chaves de criptografia (JWT_ENCRYPTION_KEY, JWT_*_SECRET)</item>
        /// <item>- Configurações de emissor/audience (JWT_ISSUER, JWT_AUDIENCE)</item>
        /// <item>- Tempos de validade (JWT_*_TOKEN_MINUTES)</item>
        /// <item>- Paths para cookies (JWT_*_TOKEN_PATH)</item>
        /// </list>
        /// </remarks>
        public static JwtSettingsSingleton GetJwtSettingsSingleton(IConfiguration config)
        {
            return new JwtSettingsSingleton(new JwtSettings
            {
                EncryptionKey = ConfigUtils.GetRequiredEnvVar("JWT_ENCRYPTION_KEY"),
                LoginSecretKey = ConfigUtils.GetRequiredEnvVar("JWT_LOGIN_SECRET"),
                AccessSecretKey = ConfigUtils.GetRequiredEnvVar("JWT_ACCESS_SECRET"),
                RefreshSecretKey = ConfigUtils.GetRequiredEnvVar("JWT_REFRESH_SECRET"),
                Domain = ConfigUtils.GetRequiredEnvVar("JWT_DOMAIN"),

                Issuer = GetJwtSettings("Issuer", config),
                Audience = GetJwtSettings("Audience", config),
                AccessTokenValidityInMinutes = int.Parse(GetJwtSettings("AccessTokenValidityInMinutes", config)),
                RefreshTokenValidityInMinutes = int.Parse(GetJwtSettings("RefreshTokenValidityInMinutes", config)),
                RefreshTokenPath = GetJwtSettings("RefreshTokenPath", config),
                AccessTokenPath = GetJwtSettings("AccessTokenPath", config)
            });
        }

        /// <summary>
        /// Configura as definições de e-mail para autenticação
        /// </summary>
        /// <param name="config">Configurações do app (appsettings.json)</param>
        /// <returns>Instância pronta para uso das configurações de e-mail</returns>
        /// <exception cref="InvalidOperationException">
        /// Erro se faltar configurações no appsettings ou variáveis de ambiente
        /// </exception>
        /// <remarks>
        /// Busca configurações em:
        /// - appsettings.json (servidor, porta, remetente)
        /// - Variáveis de ambiente (usuário/senha SMTP)
        /// </remarks>
        public static AuthEmailSettingsSingleton GetAuthEmailSettingsSingleton(IConfiguration config)
        {
            return new AuthEmailSettingsSingleton(new EmailSettings
            {
                SmtpServer = GetAuthEmailSettings("SmtpServer", config),
                SmtpPort = int.Parse(GetAuthEmailSettings("SmtpPort", config)),
                SenderName = GetAuthEmailSettings("SenderName", config),
                SenderEmail = GetAuthEmailSettings("SenderEmail", config),
                SSL = GetAuthEmailSettings("SSL", config),
                SmtpUser = ConfigUtils.GetRequiredEnvVar("AUTH_EMAIL_USER"),
                SmtpPass = ConfigUtils.GetRequiredEnvVar("AUTH_EMAIL_PASS")
            });
        }

        /// <summary>
        /// Obtém variável obrigatória do JwtSettings em appsettings
        /// </summary>
        /// <param name="name">Nome da variável</param>
        /// <param name="config">Configuração da aplicação</param>
        /// <returns>Valor da variável</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a variável não está definida
        /// </exception>
        private static string GetJwtSettings(string name, IConfiguration config) =>
            config.GetValue<string>("JwtSettings:" + name) ??
            throw new InvalidOperationException($"Environment variable '{name}' is required.");

        /// <summary>
        /// Obtém variável obrigatória do EmailSettings em appsettings
        /// </summary>
        /// <param name="name">Nome da variável</param>
        /// <param name="config">Configuração da aplicação</param>
        /// <returns>Valor da variável</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a variável não está definida
        /// </exception>
        private static string GetAuthEmailSettings(string name, IConfiguration config) =>
            config.GetValue<string>("AuthEmailSettings:" + name) ??
            throw new InvalidOperationException($"Environment variable '{name}' is required.");
    }
}
