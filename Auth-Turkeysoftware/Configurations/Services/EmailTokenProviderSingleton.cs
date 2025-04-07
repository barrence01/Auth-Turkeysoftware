using Auth_Turkeysoftware.Configurations.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Auth_Turkeysoftware.Configurations.Services
{
    //TODO Adicionar método para obter credencias de variáveis de ambiente
    public sealed class EmailTokenProviderSingleton
    {
        private readonly EmailTokenProviderSettings Settings;
        public EmailTokenProviderSingleton(IOptions<EmailTokenProviderSettings> options)
        {
            Settings = options.Value;
        }

        public EmailTokenProviderSettings GetSettings()
        {
            return Settings;
        }
    }
}
