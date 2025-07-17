using Auth_Turkeysoftware.Domain.Models.Result;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;

namespace Auth_Turkeysoftware.Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task RequestEnable2FAByEmail(ApplicationUser user, string email);
        Task<TwoFactorValidationResult> ConfirmEnable2FA(ApplicationUser user, string? twoFactorCode);
        Task SendConfirmEmailRequest(ApplicationUser user);
    }
}
