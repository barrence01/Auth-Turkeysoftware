using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Services.DistributedCacheService;
using Auth_Turkeysoftware.Services.MailService;

namespace Auth_Turkeysoftware.Services
{
    public class AccountRecoveryService : IAccountRecoveryService
    {
        private readonly IEmailService _emailService;
        private readonly IDistributedCacheService _cache;
        private const int LIFE_SPAN_PASS_RESET_HOURS = 3;

        public AccountRecoveryService(IEmailService emailService, IDistributedCacheService cacheService)
        {
            _emailService = emailService;
            _cache = cacheService;
        }

        /// <summary>
        /// Envia um email de redefinição de senha para o usuário.
        /// </summary>
        /// <param name="resetToken">O token de redefinição de senha.</param>
        /// <param name="userEmail">O email do usuário que receberá o link de redefinição de senha.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        public async Task SendPasswordResetEmail(string resetToken, string userEmail)
        {
            string passResetCacheKey = GetPassResetCacheKey(userEmail);

            if (await _cache.IsCachedAsync(passResetCacheKey)) {
                return;
            }

            await _cache.SetAsync(passResetCacheKey, "true", TimeSpan.FromHours(LIFE_SPAN_PASS_RESET_HOURS));

            var resetLink = $"https://yourfrontend.com/reset-password?token={Uri.EscapeDataString(resetToken)}&email={userEmail}";
            var emailRequest = new SendEmailDTO
            {
                Subject = "Recuperação de senha - TurkeySoftware",
                Body = $"Clique no link para resetar sua senha: {resetLink}"
            };

            emailRequest.To.Add(userEmail);

            //await _emailService.SendEmailAsync(emailRequest);
        }

        /// <summary>
        /// Gera uma chave para ser utilizada nas operações com cache para validação de dois fatores
        /// </summary>
        /// <param name="email">Email ou Username do usuário.</param>
        /// <returns>Uma string contendo a chave a ser utilizada.</returns>
        private static string GetPassResetCacheKey(string email)
        {
            return $"PassReset:{email}";
        }
    }
}
