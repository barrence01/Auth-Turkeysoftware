using Auth_Turkeysoftware.Configurations.Models;
using Microsoft.Extensions.Options;

namespace Auth_Turkeysoftware.Configurations.Services
{
    public class EmailSettingsSingleton
    {
        private readonly EmailSettings _emailSettings;
        public EmailSettingsSingleton(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public EmailSettings GetEmailSettings()
        {
            return _emailSettings;
        }
    }
}
