using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services.DistributedCacheService;
using Auth_Turkeysoftware.Services.MailService;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace Auth_Turkeysoftware.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEmailService _emailService;
        private readonly IDistributedCacheService _cache;
        private readonly ILogger<AuthenticationService> _logger;
        private const int TWO_FACTOR_CODE_LIFE_LIMIT = 10;

        public AuthenticationService(IEmailService emailService, IDistributedCacheService cacheService, ILogger<AuthenticationService> logger)
        {
            _emailService = emailService;
            _cache = cacheService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task SendTwoFactorCodeAsync(string email)
        {
            string code = RandomNumberGenerator.GetInt32(1000000, 9999999).ToString();
            TwoFactorDTO twoFactorDto = new TwoFactorDTO { TwoFactorCode = code };

            string cacheKey = Get2FACacheKey(email);
            await _cache.SetAsync(cacheKey, twoFactorDto, TimeSpan.FromMinutes(TWO_FACTOR_CODE_LIFE_LIMIT));

            var emailRequest = new SendEmailDTO
            {
                Subject = "Código de autenticação 2FA - TurkeySoftware",
                Body = $"Seu código de autenticação é: <b>{code}</b>. Este código irá expirar em 5 minutos."
            };
            emailRequest.To.Add(email);

            //await _emailService.SendEmailAsync(emailRequest);
        }

        /// <inheritdoc/>
        public async Task<TwoFactorValidationDTO> VerifyTwoFactor(ApplicationUser user, string? twoFactorCode)
        {
            var result = new TwoFactorValidationDTO();

            if (!user.TwoFactorEnabled) {
                return result;
            }

            if (string.IsNullOrEmpty(twoFactorCode)) {
                result.IsTwoFactorCodeEmpty = true;
                return result;
            }

            TwoFactorDTO? storedTwoFactorDto = await _cache.GetAsync<TwoFactorDTO>(Get2FACacheKey(user.Email));
            if (storedTwoFactorDto == null) {
                result.IsTwoFactorCodeExpired = true;
                return result;
            }

            storedTwoFactorDto.NumberOfTries += 1;
            await _cache.SetAsync(Get2FACacheKey(user.Email), storedTwoFactorDto, TimeSpan.FromMinutes(TWO_FACTOR_CODE_LIFE_LIMIT));

            if (storedTwoFactorDto.NumberOfTries >= 5) {
                result.IsMaxNumberOfTriesExceeded = true;
                return result;
            }

            if (storedTwoFactorDto.TwoFactorCode == twoFactorCode) {
                await _cache.RemoveAsync(Get2FACacheKey(user.Email));
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
