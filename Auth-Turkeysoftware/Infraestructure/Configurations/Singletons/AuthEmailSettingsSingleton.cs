using Auth_Turkeysoftware.Infraestructure.Configurations.Models;

namespace Auth_Turkeysoftware.Infraestructure.Configurations.Singletons
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
