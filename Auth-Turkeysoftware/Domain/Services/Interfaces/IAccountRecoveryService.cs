using Auth_Turkeysoftware.Domain.Models.Result;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Microsoft.AspNetCore.Identity.Data;

namespace Auth_Turkeysoftware.Domain.Services.Interfaces
{
    public interface IAccountRecoveryService
    {
        Task SendPasswordResetEmail(ApplicationUser user);
        Task<ResetPasswordValidationResult> ResetPassword(ApplicationUser user, ResetPasswordRequest request);
    }
}
