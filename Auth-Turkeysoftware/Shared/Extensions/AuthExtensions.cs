using Auth_Turkeysoftware.Shared.Constants;
using Auth_Turkeysoftware.Shared.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Auth_Turkeysoftware.Shared.Extensions
{
    /// <summary>
    /// Extensões para configuração de autenticação JWT
    /// </summary>
    public static class AuthExtensions
    {
        /// <summary>
        /// Configura a autenticação JWT na aplicação
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        /// <param name="config">Configuração da aplicação</param>
        /// <returns>IServiceCollection para encadeamento</returns>
        /// <remarks>
        /// Requer as seguintes variáveis de ambiente:
        /// - JWT_ACCESS_TOKEN_PATH: Caminho do cookie
        /// - JWT_ACCESS_SECRET: Chave de assinatura
        /// - JWT_ENCRYPTION_KEY: Chave de criptografia
        /// 
        /// Requer variáveis do appsettings
        /// - JwtSettings:Audience: Audience do token
        /// - JwtSettings:Issuer: Emissor do token  
        /// - JwtSettings:AccessTokenPath: Domínio path do cookie
        /// </remarks>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var domain = ConfigUtils.GetRequiredEnvVar("JWT_DOMAIN");
            var accessSecret = ConfigUtils.GetRequiredEnvVar("JWT_ACCESS_SECRET");
            var encryptionKey = ConfigUtils.GetRequiredEnvVar("JWT_ENCRYPTION_KEY");

            var audience = GetJwtSettings("Audience", config);
            var issuer = GetJwtSettings("Issuer", config);
            var accessPath = GetJwtSettings("AccessTokenPath", config);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudience = audience,
                    ValidIssuer = issuer,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessSecret)),
                    TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encryptionKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies[TokenNameConstant.ACCESS_TOKEN];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Cookies.Delete(TokenNameConstant.ACCESS_TOKEN, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                IsEssential = true,
                                SameSite = SameSiteMode.None,
                                Domain = domain,
                                Path = accessPath
                            });
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        /// <summary>
        /// Obtém variável obrigatória do appsettings
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
    }
}
