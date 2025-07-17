using Auth_Turkeysoftware.Domain.Models.VOs;
using Auth_Turkeysoftware.Domain.Services.Interfaces;
using Auth_Turkeysoftware.Infraestructure.Mail;

namespace Auth_Turkeysoftware.Domain.Services.Implementations
{
    public class CommunicationService : ICommunicationService
    {
        private readonly EmailService _emailService;
        public CommunicationService(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Send2FAEmailAsync(string email, string twoFactorCode, string tokenLifeSpanInMinutes)
        {
            var emailRequest = new SendEmailVO
            {
                Subject = "Código de autenticação 2FA - TurkeySoftware",
                Body = $"Seu código de autenticação é: <b>{twoFactorCode}</b>. Este código irá expirar em {tokenLifeSpanInMinutes} minutos."
            };
            emailRequest.To.Add(email);
            await _emailService.SendEmailAsync(emailRequest);
        }

        public async Task SendEnable2FAEmailAsync(string email, string twoFactorCode, string tokenLifeSpanInMinutes)
        {
            var emailRequest = new SendEmailVO
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
            var emailRequest = new SendEmailVO
            {
                Subject = "Recuperação de senha - TurkeySoftware",
                Body = $"Clique no link para resetar sua senha: {resetLink}"
            };

            emailRequest.To.Add(email);
            await _emailService.SendEmailAsync(emailRequest);
        }

        public async Task SendConfirmEmailRequest(string userId, string email, string confirmToken)
        {
            var confirmLink = $"https://yourfrontend.com/confirm-email?token={Uri.EscapeDataString(confirmToken)}&email={email}&userId={userId}";
            var emailRequest = new SendEmailVO
            {
                Subject = "Confirmação de email - TurkeySoftware",
                Body = $"Clique no link para confirmar seu email: {confirmLink}"
            };

            emailRequest.To.Add(email);
            await _emailService.SendEmailAsync(emailRequest);
        }
    }
}
