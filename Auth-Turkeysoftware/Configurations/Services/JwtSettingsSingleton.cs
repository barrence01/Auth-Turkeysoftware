using Auth_Turkeysoftware.Configurations.Models;
using Microsoft.Extensions.Options;

namespace Auth_Turkeysoftware.Configurations.Services
{
    //TODO Adicionar método para obter credencias de variáveis de ambiente
    public sealed class JwtSettingsSingleton
    {
        private readonly JwtSettings _jwtSettings;
        public JwtSettingsSingleton(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public JwtSettings GetJwtSettings()
        {
            return _jwtSettings;
        }

        public string GetJwtAccessSecretKey()
        {
            return GetJwtSettings().AccessSecretKey;
        }

        public string GetJwtRefreshSecretKey()
        {
            return GetJwtSettings().RefreshSecretKey;
        }

        public string GetJwtDomain()
        {
            return GetJwtSettings().Domain;
        }

        public string GetJwtIssuer()
        {
            return GetJwtSettings().Domain;
        }

        public string GetJwtAudience()
        {
            return GetJwtSettings().Domain;
        }

        public int GetRefreshTokenValidityInMinutes()
        {
            return GetJwtSettings().RefreshTokenValidityInMinutes;
        }

        public int GetAccessTokenValidityInMinutes()
        {
            return GetJwtSettings().AccessTokenValidityInMinutes;
        }
        public string GetRefreshTokenPath()
        {
            return GetJwtSettings().RefreshTokenPath;
        }
        public string GetAccessTokenPath()
        {
            return GetJwtSettings().AccessTokenPath;
        }
    }
}
