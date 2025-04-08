using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Services.MailService;

namespace Auth_Turkeysoftware.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IEmailService _emailService;
        public CommunicationService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Send2FAEmailAsync(string email, string twoFactorCode, string tokenLifeSpanInMinutes)
        {
            var emailRequest = new SendEmailDto
            {
                Subject = "Código de autenticação 2FA - TurkeySoftware",
                Body = $"Seu código de autenticação é: <b>{twoFactorCode}</b>. Este código irá expirar em {tokenLifeSpanInMinutes} minutos."
            };
            emailRequest.To.Add(email);

            await _emailService.SendEmailAsync(emailRequest);
        }

        public async Task SendEnable2FAEmailAsync(string email, string twoFactorCode, string tokenLifeSpanInMinutes)
        {
            var emailRequest = new SendEmailDto
            {
                Subject = "Código de ativação do 2FA - TurkeySoftware",
                Body = $"Seu código de ativação é: <b>{twoFactorCode}</b>. Este código irá expirar em {tokenLifeSpanInMinutes} minutos."
            };
            emailRequest.To.Add(email);

            await _emailService.SendEmailAsync(emailRequest);
        }

        public async Task SendPasswordResetEmail(string email, string resetToken)
        {
            var resetLink = $"https://yourfrontend.com/reset-password?token={Uri.EscapeDataString(resetToken)}&email={email}";
            var emailRequest = new SendEmailDto
            {
                Subject = "Recuperação de senha - TurkeySoftware",
                Body = $"Clique no link para resetar sua senha: {resetLink}"
            };

            emailRequest.To.Add(email);

            await _emailService.SendEmailAsync(emailRequest);
        }
    }
}
