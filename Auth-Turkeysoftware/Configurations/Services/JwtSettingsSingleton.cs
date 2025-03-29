using Auth_Turkeysoftware.Configurations.Models;
using Microsoft.Extensions.Options;

namespace Auth_Turkeysoftware.Configurations.Services
{
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
    }
}
