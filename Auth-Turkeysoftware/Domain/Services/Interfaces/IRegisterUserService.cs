using Auth_Turkeysoftware.API.Models.Request;
using Auth_Turkeysoftware.Domain.Models.Result;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;

namespace Auth_Turkeysoftware.Domain.Services.Interfaces
{
    public interface IRegisterUserService
    {
        Task<RegisterUserResult> RegisterUser(RegisterRequest model);
        Task SendConfirmEmailRequest(ApplicationUser user);
        Task<bool> ConfirmEmailRequest(ApplicationUser user, string token);
    }
}
