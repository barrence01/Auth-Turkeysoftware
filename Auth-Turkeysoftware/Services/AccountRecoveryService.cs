using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Result;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services.DistributedCacheService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace Auth_Turkeysoftware.Services
{
    public class AccountRecoveryService : IAccountRecoveryService
    {
        private readonly ICommunicationService _commService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDistributedCacheService _cache;
        private const int RESET_CODE_LIFE_IN_HOURS = 3;

        public AccountRecoveryService(ICommunicationService communicationService, UserManager<ApplicationUser> userManager, IDistributedCacheService cacheService)
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
            string passResetKey = GetPassResetKey(user.Email!);

            if (await _cache.IsCachedAsync(passResetKey)) {
                return;
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            TwoFactorRetryDto retryInfo = new TwoFactorRetryDto { UserId = user.Id, TwoFactorCode = resetToken, MaxNumberOfTries = 15 };

            await _cache.SetAsync(passResetKey, retryInfo, TimeSpan.FromHours(RESET_CODE_LIFE_IN_HOURS));

            await _commService.SendPasswordResetEmail(user.Email!, resetToken);
        }

        public async Task<ResetPasswordValidationResult> ResetPassword(ApplicationUser user, ResetPasswordRequest request)
        {
            if (user == null) {
                throw new ArgumentNullException(nameof(user),"Usuário não pode ser nulo.");
            }

            ResetPasswordValidationResult result = new ResetPasswordValidationResult();
            if (string.IsNullOrEmpty(request.ResetCode)) {
                result.IsResetCodeEmpty = true;
                return result;
            }
            else if (string.IsNullOrEmpty(request.NewPassword)) { 
                result.IsNewPasswordEmpty = true;
                return result;
            }

            string cacheKey = GetPassResetKey(user.UserName!);
            TwoFactorRetryDto? retryInfo = await _cache.GetAsync<TwoFactorRetryDto>(cacheKey);
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

        /// <summary>
        /// Gera uma chave para ser utilizada nas operações com cache para validação de dois fatores
        /// </summary>
        /// <param name="email">Email ou Username do usuário.</param>
        /// <returns>Uma string contendo a chave a ser utilizada.</returns>
        private static string GetPassResetKey(string email)
        {
            return $"PassReset:{email}";
        }
    }
}
