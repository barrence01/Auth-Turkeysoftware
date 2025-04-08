using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Models.Result;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Services
{
    public interface IRegisterUserService
    {
        Task<RegisterUserResult> RegisterUser(RegisterRequest model);
        Task SendConfirmEmailRequest(ApplicationUser user);
        Task<bool> ConfirmEmailRequest(ApplicationUser user, string token);
    }
}
