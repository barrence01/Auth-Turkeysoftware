using Auth_Turkeysoftware.Infraestructure.Configurations.Models;
using Microsoft.Extensions.Options;

namespace Auth_Turkeysoftware.Infraestructure.Configurations.Singletons
{
    public sealed class JwtSettingsSingleton
    {
        private readonly JwtSettings _jwtSettings;
        public JwtSettingsSingleton(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public JwtSettings GetJwtSettings()
        {
            return _jwtSettings;
        }

        public string GetJwtLoginSecretKey()
        {
            return GetJwtSettings().LoginSecretKey;
        }

        public string GetJwtAccessSecretKey()
        {
            return GetJwtSettings().AccessSecretKey;
        }

        public string GetJwtRefreshSecretKey()
        {
            return GetJwtSettings().RefreshSecretKey;
        }

        public string GetJwtEncryptionKey()
        {
            return GetJwtSettings().EncryptionKey;
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
