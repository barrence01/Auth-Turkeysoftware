using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.Configurations;
using Microsoft.Extensions.Options;
using Serilog;

namespace Auth_Turkeysoftware.Services
{
    public sealed class JwtSettingsSingleton
    {
        private readonly JwtSettings _jwtSettings;
        public JwtSettingsSingleton(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public JwtSettings GetJwtSettings() {
            return _jwtSettings;
        }
    }
}
