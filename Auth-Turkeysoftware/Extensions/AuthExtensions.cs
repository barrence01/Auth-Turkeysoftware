using Auth_Turkeysoftware.Enums.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Auth_Turkeysoftware.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var audience = GetRequiredEnvVar("JWT_AUDIENCE");
            var issuer = GetRequiredEnvVar("JWT_ISSUER");
            var domain = GetRequiredEnvVar("JWT_DOMAIN");
            var accessPath = GetRequiredEnvVar("JWT_ACCESS_TOKEN_PATH");
            var accessSecret = GetRequiredEnvVar("JWT_ACCESS_SECRET");
            var encryptionKey = GetRequiredEnvVar("JWT_ENCRYPTION_KEY");

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
                        if (!string.IsNullOrEmpty(token)) {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException) {
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

        private static string GetRequiredEnvVar(string name) =>
            Environment.GetEnvironmentVariable(name) ??
            throw new InvalidOperationException($"Environment variable '{name}' is required.");
    }
}
