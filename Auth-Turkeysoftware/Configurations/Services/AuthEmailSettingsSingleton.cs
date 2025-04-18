using Auth_Turkeysoftware.Configurations.Models;
using Microsoft.Extensions.Options;

namespace Auth_Turkeysoftware.Configurations.Services
{
    //TODO Adicionar método para obter credencias de variáveis de ambiente
    public sealed class AuthEmailSettingsSingleton
    {
        private readonly EmailSettings _emailSettings;
        public AuthEmailSettingsSingleton(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }

        public EmailSettings GetEmailSettings()
        {
            return _emailSettings;
        }
    }
}
