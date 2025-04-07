using Auth_Turkeysoftware.Configurations.Models;
using Microsoft.Extensions.Options;

namespace Auth_Turkeysoftware.Configurations.Services
{
    //TODO Adicionar método para obter credencias de variáveis de ambiente
    public sealed class EmailSettingsSingleton
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
