using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Results;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services.DistributedCacheService;
using Auth_Turkeysoftware.Services.MailService;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Cryptography;
using YamlDotNet.Core.Tokens;

namespace Auth_Turkeysoftware.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IEmailService _emailService;
        private readonly IDistributedCacheService _cache;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly ITwoFactorRepository _twoFactorRepository;
        private readonly EmailTokenProviderSingleton _emailTokenSettings;

        public AuthenticationService(IEmailService emailService, IDistributedCacheService cacheService, ILogger<AuthenticationService> logger,
                                     ITwoFactorRepository twoFactorRepository, EmailTokenProviderSingleton emailTokenSettings)
        {
            _emailService = emailService;
            _cache = cacheService;
            _logger = logger;
            _twoFactorRepository = twoFactorRepository;
            _emailTokenSettings = emailTokenSettings;
        }

        /// <inheritdoc/>
        public async Task SendTwoFactorCodeAsync(ApplicationUser user, int twoFactorMode)
        {
            switch (twoFactorMode)
            {
                case (int)TwoFactorModeEnum.EMAIL:
                    await SendTwoFactorCodeByEmail(user);
                    break;
                default:
                    throw new NotImplementedException("Tipo de autenticação 2FA não implementada.");
            }
        }

        private async Task SendTwoFactorCodeByEmail(ApplicationUser user) 
        {
            string cacheKey = Get2FACacheKey(user.UserName!);

            if (await _cache.IsCachedAsync(cacheKey))
            {
                return;
            }

            string token = RandomNumberGenerator.GetInt32(1000000, 9999999).ToString();
            var tokenLifeSpanInMinutes = _emailTokenSettings.GetSettings().TokenLifeSpan;
            var maxNumberOfTries = _emailTokenSettings.GetSettings().MaxNumberOfTries;

            TwoFactorRetryDTO retryInfo = new TwoFactorRetryDTO { UserId = user.Id, TwoFactorCode = token, MaxNumberOfTries = maxNumberOfTries };


            await _cache.SetAsync(cacheKey, retryInfo, tokenLifeSpanInMinutes);

            var emailRequest = new SendEmailDTO
            {
                Subject = "Código de autenticação 2FA - TurkeySoftware",
                Body = $"Seu código de autenticação é: <b>{token}</b>. Este código irá expirar em {tokenLifeSpanInMinutes} minutos."
            };
            emailRequest.To.Add(user.UserName!);

            //await _emailService.SendEmailAsync(emailRequest);
        }

        /// <inheritdoc/>
        public async Task<TwoFactorValidationResult> VerifyTwoFactorAuthentication(ApplicationUser user, string? twoFactorCode)
        {
            if (user.UserName == null) { 
                throw new ArgumentNullException("Nome de usuário não pode ser nulo.");
            }

            var result = new TwoFactorValidationResult();

            if (!user.TwoFactorEnabled) {
                return result;
            }

            if (string.IsNullOrEmpty(twoFactorCode)) {
                result.IsTwoFactorCodeEmpty = true;
                return result;
            }

            string cacheKey = Get2FACacheKey(user.UserName);
            TwoFactorRetryDTO? retryInfo = await _cache.GetAsync<TwoFactorRetryDTO>(cacheKey);
            if (retryInfo == null) {
                result.IsTwoFactorCodeExpired = true;
                return result;
            }

            retryInfo.NumberOfTries += 1;
            await _cache.SetAsync(cacheKey, retryInfo);

            if (retryInfo.NumberOfTries >= retryInfo.MaxNumberOfTries) {
                await _cache.RemoveAsync(cacheKey);
                result.IsMaxNumberOfTriesExceeded = true;
                return result;
            }

            if (retryInfo.TwoFactorCode == twoFactorCode && user.Id == retryInfo.UserId) {
                await _cache.RemoveAsync(cacheKey);
                return result;
            }
            else {
                result.IsTwoFactorCodeInvalid = true;
                return result;
            }
        }

        public async Task<List<TwoFactorAuthResponse>> ListActive2FAOptions(ApplicationUser user) {
            var twoFactorOptions = await _twoFactorRepository.ListActive2FAOptionsAsync(user.Id);

            List<TwoFactorAuthResponse> list = new List<TwoFactorAuthResponse>();
            foreach (var twoFactorOption in twoFactorOptions)
            {
                TwoFactorAuthResponse option = new TwoFactorAuthResponse();
                option.TwoFactorMode = twoFactorOption.TwoFactorMode;

                switch (twoFactorOption.TwoFactorMode) {
                    case (int)TwoFactorModeEnum.EMAIL:
                        option.To = user.UserName;
                        break;
                    case (int)TwoFactorModeEnum.WHATSAPP:
                    case (int)TwoFactorModeEnum.SMS:
                        option.To = user.PhoneNumber;
                        break;
                    default:
                            break;
                }
                list.Add(option);
            }
            return list;
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
