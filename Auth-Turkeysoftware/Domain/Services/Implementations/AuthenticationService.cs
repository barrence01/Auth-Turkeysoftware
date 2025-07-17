using Auth_Turkeysoftware.Infraestructure.Configurations.Singletons;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Interfaces;
using Auth_Turkeysoftware.Infraestructure.DistributedCache;
using System.Security.Cryptography;
using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Shared.Enums;
using Auth_Turkeysoftware.Domain.Models.Result;
using Auth_Turkeysoftware.Domain.Models.VOs;
using Auth_Turkeysoftware.Domain.Services.Interfaces;
using Auth_Turkeysoftware.Shared.Utils;

namespace Auth_Turkeysoftware.Domain.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICommunicationService _commService;
        private readonly IDistributedCache _cache;
        private readonly ITwoFactorRepository _twoFactorRepository;
        private readonly EmailTokenProviderSingleton _emailTokenSettings;

        public AuthenticationService(ICommunicationService communicationService, IDistributedCache cacheService,
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
            string cacheKey = AuthUtil.Get2FACacheKey(user.UserName!);

            if (await _cache.IsCachedAsync(cacheKey))
            {
                return;
            }

            int initialRange = _emailTokenSettings.GetSettings().TokenInitialRange;
            int finalRange = _emailTokenSettings.GetSettings().TokenFinalRange;

            string token = RandomNumberGenerator.GetInt32(initialRange, finalRange).ToString();
            var tokenLifeSpanInMinutes = _emailTokenSettings.GetSettings().TokenLifeSpan;
            var maxNumberOfTries = _emailTokenSettings.GetSettings().MaxNumberOfTries;

            TwoFactorRetryVO retryInfo = new TwoFactorRetryVO { UserId = user.Id, TwoFactorCode = token, MaxNumberOfTries = maxNumberOfTries };


            await _cache.SetAsync(cacheKey, retryInfo, tokenLifeSpanInMinutes);

            await _commService.Send2FAEmailAsync(user.UserName!, token, tokenLifeSpanInMinutes.ToString());
        }

        /// <inheritdoc/>
        public async Task<TwoFactorValidationResult> VerifyTwoFactorAuthentication(ApplicationUser user, string? twoFactorCode)
        {
            if (user.UserName == null)
            {
                throw new ArgumentNullException(nameof(user), "Nome de usuário não pode ser nulo.");
            }

            var result = new TwoFactorValidationResult();

            if (!user.TwoFactorEnabled)
            {
                return result;
            }

            if (string.IsNullOrEmpty(twoFactorCode))
            {
                result.IsTwoFactorCodeEmpty = true;
                return result;
            }

            string cacheKey = AuthUtil.Get2FACacheKey(user.UserName);
            TwoFactorRetryVO? retryInfo = await _cache.GetAsync<TwoFactorRetryVO>(cacheKey);
            if (retryInfo == null)
            {
                result.IsTwoFactorCodeExpired = true;
                return result;
            }

            retryInfo.NumberOfTries += 1;
            await _cache.SetAsync(cacheKey, retryInfo);

            if (retryInfo.NumberOfTries >= retryInfo.MaxNumberOfTries)
            {
                await _cache.RemoveAsync(cacheKey);
                result.IsMaxNumberOfTriesExceeded = true;
                return result;
            }

            if (retryInfo.TwoFactorCode == twoFactorCode && user.Id == retryInfo.UserId)
            {
                await _cache.RemoveAsync(cacheKey);
                return result;
            }
            else
            {
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
                To = option.TwoFactorMode switch
                {
                    (int)TwoFactorModeEnum.EMAIL => user.UserName,
                    (int)TwoFactorModeEnum.WHATSAPP or (int)TwoFactorModeEnum.SMS => user.PhoneNumber,
                    _ => null
                }
            }).ToList();
        }
    }
}
