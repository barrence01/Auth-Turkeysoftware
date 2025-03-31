using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Services.MailService;

namespace Auth_Turkeysoftware.Services
{
    public class AccountRecoveryService : IAccountRecoveryService
    {
        private readonly IEmailService _emailService;

        public AccountRecoveryService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Envia um email de redefinição de senha para o usuário.
        /// </summary>
        /// <param name="resetToken">O token de redefinição de senha.</param>
        /// <param name="userEmail">O email do usuário que receberá o link de redefinição de senha.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        public async Task SendPasswordResetEmail(string resetToken, string userEmail)
        {

            var resetLink = $"https://yourfrontend.com/reset-password?token={Uri.EscapeDataString(resetToken)}&email={userEmail}";
            var emailRequest = new SendEmailDTO
            {
                Subject = "Recuperação de senha - TurkeySoftware",
                Body = $"Clique no link para resetar sua senha: {resetLink}"
            };

            emailRequest.To.Add(userEmail);

            await _emailService.SendEmailAsync(emailRequest);
        }
    }
}
