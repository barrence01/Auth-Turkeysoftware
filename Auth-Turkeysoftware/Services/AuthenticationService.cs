using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services.MailService;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace Auth_Turkeysoftware.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(IEmailService emailService, IMemoryCache cache, ILogger<AuthenticationService> logger)
        {
            _emailService = emailService;
            _cache = cache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task SendTwoFactorCodeAsync(string email)
        {
            string code = RandomNumberGenerator.GetInt32(1000000, 9999999).ToString();
            TwoFactorAuthDTO twoFactorDto = new TwoFactorAuthDTO(code);

            string cacheKey = Get2FACacheKey(email);
            _cache.Set(cacheKey, twoFactorDto, TimeSpan.FromMinutes(5));

            var emailRequest = new SendEmailDTO
            {
                Subject = "Código de autenticação 2FA - TurkeySoftware",
                Body = $"Seu código de autenticação é: <b>{code}</b>. Este código irá expirar em 5 minutos."
            };
            emailRequest.To.Add(email);

            await _emailService.SendEmailAsync(emailRequest);
        }

        /// <inheritdoc/>
        public TwoFactorValidationDTO VerifyTwoFactor(ApplicationUser user, string? twoFactorCode)
        {
            var result = new TwoFactorValidationDTO();

            if (!user.TwoFactorEnabled) {
                return result;
            }

            if (string.IsNullOrEmpty(twoFactorCode)) {
                result.IsTwoFactorCodeEmpty = true;
                return result;
            }

            string cacheKey = Get2FACacheKey(user.Email);
            if (!_cache.TryGetValue(cacheKey, out TwoFactorAuthDTO storedTwoFactorDto) || storedTwoFactorDto == null) {
                result.IsTwoFactorCodeExpired = true;
                return result;
            }

            storedTwoFactorDto.NumberOfTries += 1;

            if (storedTwoFactorDto.NumberOfTries >= 5) {
                result.IsMaxNumberOfTriesExceeded = true;
                return result;
            }

            storedTwoFactorDto.NumberOfTries += 1;

            if (storedTwoFactorDto.TwoFactorCode == twoFactorCode) {
                _cache.Remove(cacheKey);
                return result;
            }
            else {
                result.IsTwoFactorCodeInvalid = true;
                return result;
            }
        }

        /// <summary>
        /// Gera uma chave para ser utilizada nas operações com cache para validação de dois fatores
        /// </summary>
        /// <param name="email">Email ou Username do usuário.</param>
        /// <returns>Uma string contendo a chave a ser utilizada.</returns>
        private static string Get2FACacheKey(string email) {
            return $"2FA:{email}";
        }
    }
}
