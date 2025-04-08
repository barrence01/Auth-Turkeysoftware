using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Helpers;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Results;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Repositories.Context;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services.DistributedCacheService;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Auth_Turkeysoftware.Services
{
    public class UserService : IUserService
    {
        private readonly ICommunicationService _commService;
        private readonly IDistributedCacheService _cache;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailTokenProviderSingleton _emailTokenSettings;
        private readonly ITwoFactorRepository _twoFactorRepository;
        internal AppDbContext _dbContext;

        public UserService(ICommunicationService communicationService, IDistributedCacheService cacheService, UserManager<ApplicationUser> userManager,
                           EmailTokenProviderSingleton emailTokenSettings, ITwoFactorRepository twoFactorRepository, AppDbContext dbContext)
        {
            _commService = communicationService;
            _cache = cacheService;
            _userManager = userManager;
            _emailTokenSettings = emailTokenSettings;
            _twoFactorRepository = twoFactorRepository;
            _dbContext = dbContext;
        }

        public async Task RequestEnable2FAByEmail(ApplicationUser user, string email)
        {
            string cacheKey = Get2FAEnableCacheKey(email);

            if (await _cache.IsCachedAsync(cacheKey)) {
                return;
            }

            var token = RandomNumberGenerator.GetInt32(1000000, 9999999).ToString();
            var tokenLifeSpanInMinutes = _emailTokenSettings.GetSettings().TokenLifeSpan;
            var maxNumberOfTries = _emailTokenSettings.GetSettings().MaxNumberOfTries;

            TwoFactorRetryDto retryInfo = new TwoFactorRetryDto { UserId = user.Id, TwoFactorCode = token, MaxNumberOfTries = maxNumberOfTries };

            await _cache.SetAsync(cacheKey, retryInfo, tokenLifeSpanInMinutes);

            await _commService.SendEnable2FAEmailAsync(email, token, tokenLifeSpanInMinutes.ToString());
        }

        /// <inheritdoc/>
        public async Task<TwoFactorValidationResult> ConfirmEnable2FA(ApplicationUser user, string? twoFactorCode)
        {
            if (user == null) {
                throw new ArgumentNullException(nameof(user),"Usuário não pode ser nulo.");
            }

            var result = new TwoFactorValidationResult();

            if (string.IsNullOrEmpty(twoFactorCode)) {
                result.IsTwoFactorCodeEmpty = true;
                return result;
            }

            string cacheKey = Get2FAEnableCacheKey(user.UserName!);
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
                await Enable2FA(user);
                return result;
            }
            else {
                result.IsTwoFactorCodeInvalid = true;
                return result;
            }
        }

        private async Task Enable2FA(ApplicationUser user)
        {
            if (user == null) {
                throw new ArgumentNullException(nameof(user),"usuário não pode ser nulo.");
            }

            TwoFactorAuthModel model = new TwoFactorAuthModel(user.Id, (int)TwoFactorModeEnum.EMAIL);

            await TransactionHelper.ExecuteWithTransactionAsync(_dbContext, async () =>
            {
                await _twoFactorRepository.AddTwoFactorAuth(model);
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                return true;
            });
        }


        private static string Get2FAEnableCacheKey(string email)
        {
            return $"2FA-Email-Enable:{email}";
        }
    }
}
