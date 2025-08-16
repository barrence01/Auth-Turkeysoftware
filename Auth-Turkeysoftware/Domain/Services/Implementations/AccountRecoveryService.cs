using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.Infraestructure.DistributedCache;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Auth_Turkeysoftware.Domain.Models.Result;
using Auth_Turkeysoftware.Domain.Models.VOs;
using Auth_Turkeysoftware.Domain.Services.Interfaces;
using Auth_Turkeysoftware.Shared.Utils;

namespace Auth_Turkeysoftware.Domain.Services.Implementations
{
    public class AccountRecoveryService : IAccountRecoveryService
    {
        private readonly ICommunicationService _commService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDistributedCache _cache;
        private const int RESET_CODE_LIFESPAN_IN_HOURS = 3;

        public AccountRecoveryService(ICommunicationService communicationService, UserManager<ApplicationUser> userManager, IDistributedCache cacheService)
        {
            _commService = communicationService;
            _userManager = userManager;
            _cache = cacheService;
        }

        /// <summary>
        /// Envia um email de redefinição de senha para o usuário.
        /// </summary>
        /// <param name="user">O objeto que representa o usuário.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        public async Task SendPasswordResetEmail(ApplicationUser user)
        {
            string passResetKey = AuthUtil.GetPassResetKey(user.Email!);

            if (await _cache.IsCachedAsync(passResetKey)) {
                return;
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            TwoFactorRetryVO retryInfo = new TwoFactorRetryVO { UserId = user.Id, TwoFactorCode = resetToken, MaxNumberOfTries = 15 };

            await _cache.SetAsync(passResetKey, retryInfo, TimeSpan.FromHours(RESET_CODE_LIFESPAN_IN_HOURS));

            await _commService.SendPasswordResetEmail(user.Email!, resetToken);
        }

        public async Task<ResetPasswordValidationResult> ResetPassword(ApplicationUser user, ResetPasswordRequest request)
        {
            if (user == null) {
                throw new ArgumentNullException(nameof(user), "Usuário não pode ser nulo.");
            }

            ResetPasswordValidationResult result = new();

            if (string.IsNullOrEmpty(request.ResetCode)) {
                result.IsResetCodeEmpty = true;
                return result;
            } else if (string.IsNullOrEmpty(request.NewPassword)) {
                result.IsNewPasswordEmpty = true;
                return result;
            }

            string cacheKey = AuthUtil.GetPassResetKey(user.UserName!);
            TwoFactorRetryVO? retryInfo = await _cache.GetAsync<TwoFactorRetryVO>(cacheKey);
            if (retryInfo == null) {
                result.IsResetCodeExpired = true;
                return result;
            }

            retryInfo.NumberOfTries += 1;
            await _cache.SetAsync(cacheKey, retryInfo);

            if (retryInfo.NumberOfTries >= retryInfo.MaxNumberOfTries) {
                await _cache.RemoveAsync(cacheKey);
                result.IsResetCodeExpired = true;
                return result;
            }

            var passResetResult = await _userManager.ResetPasswordAsync(user, request.ResetCode, request.NewPassword);

            if (!passResetResult.Succeeded) {
                result.Errors = (List<IdentityError>)passResetResult.Errors;
                return result;
            }

            return result;
        }
    }
}
