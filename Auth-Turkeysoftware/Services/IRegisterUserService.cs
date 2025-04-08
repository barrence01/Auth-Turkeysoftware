using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Models.Result;

namespace Auth_Turkeysoftware.Services
{
    public interface IRegisterUserService
    {
        Task<RegisterUserResult> RegisterUser(RegisterRequest model);
    }
}
