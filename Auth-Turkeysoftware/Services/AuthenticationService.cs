using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Result;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services.DistributedCacheService;
using System.Security.Cryptography;

namespace Auth_Turkeysoftware.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICommunicationService _commService;
        private readonly IDistributedCacheService _cache;
        private readonly ITwoFactorRepository _twoFactorRepository;
        private readonly EmailTokenProviderSingleton _emailTokenSettings;

        public AuthenticationService(ICommunicationService communicationService, IDistributedCacheService cacheService, 
                                     ITwoFactorRepository twoFactorRepository, EmailTokenProviderSingleton emailTokenSettings)
        {
            _commService = communicationService;
            _cache = cacheService;
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

            if (await _cache.IsCachedAsync(cacheKey)) {
                return;
            }

            int initialRange = _emailTokenSettings.GetSettings().TokenInitialRange;
            int finalRange = _emailTokenSettings.GetSettings().TokenFinalRange;

            string token = RandomNumberGenerator.GetInt32(initialRange, finalRange).ToString();
            var tokenLifeSpanInMinutes = _emailTokenSettings.GetSettings().TokenLifeSpan;
            var maxNumberOfTries = _emailTokenSettings.GetSettings().MaxNumberOfTries;

            TwoFactorRetryDto retryInfo = new TwoFactorRetryDto { UserId = user.Id, TwoFactorCode = token, MaxNumberOfTries = maxNumberOfTries };


            await _cache.SetAsync(cacheKey, retryInfo, tokenLifeSpanInMinutes);

            await _commService.Send2FAEmailAsync(user.UserName!, token, tokenLifeSpanInMinutes.ToString());
        }

        /// <inheritdoc/>
        public async Task<TwoFactorValidationResult> VerifyTwoFactorAuthentication(ApplicationUser user, string? twoFactorCode)
        {
            if (user.UserName == null) { 
                throw new ArgumentNullException(nameof(user),"Nome de usuário não pode ser nulo.");
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
            TwoFactorRetryDto? retryInfo = await _cache.GetAsync<TwoFactorRetryDto>(cacheKey);
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

        public async Task<List<TwoFactorAuthResponse>> ListUserTwoFactorOptions(ApplicationUser user)
        {
            var twoFactorOptions = await _twoFactorRepository.ListActive2FAOptionsAsync(user.Id);

            return twoFactorOptions.Select(option => new TwoFactorAuthResponse
            {
                TwoFactorMode = option.TwoFactorMode,
                To = option.TwoFactorMode switch {
                                                    (int)TwoFactorModeEnum.EMAIL => user.UserName,
                                                    (int)TwoFactorModeEnum.WHATSAPP or (int)TwoFactorModeEnum.SMS => user.PhoneNumber,
                                                    _ => null
                                                 }
            }).ToList();
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
