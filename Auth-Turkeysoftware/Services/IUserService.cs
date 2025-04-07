
using Auth_Turkeysoftware.Models.Results;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Services
{
    public interface IUserService
    {
        Task RequestEnable2FAByEmail(ApplicationUser user,string userName);

        Task<TwoFactorValidationResult> ConfirmEnable2FA(ApplicationUser user, string? twoFactorCode);
    }
}
