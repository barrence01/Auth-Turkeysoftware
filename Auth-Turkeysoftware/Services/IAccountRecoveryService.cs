using Auth_Turkeysoftware.Models.Results;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.AspNetCore.Identity.Data;

namespace Auth_Turkeysoftware.Services
{
    public interface IAccountRecoveryService
    {
        Task SendPasswordResetEmail(ApplicationUser user);
        Task<ResetPasswordValidationResult> ResetPassword(ApplicationUser user, ResetPasswordRequest request);
    }
}
