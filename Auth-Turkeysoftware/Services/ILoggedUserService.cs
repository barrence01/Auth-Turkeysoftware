using Auth_Turkeysoftware.Models.DataBaseModels;
using System.IdentityModel.Tokens.Jwt;

namespace Auth_Turkeysoftware.Services
{
    public interface ILoggedUserService
    {

        Task AddLoggedUser(LoggedUserModel loggedUserModel);

        Task InvalidateUserSession(int idSessao, string idUsuario);

        Task<bool> IsBlackListed(string userId, string UserToken);

        Task UpdateSessionRefreshToken(string id, string refreshToken, string newRefreshToken);

        Task<LoggedUserModel> AddIpAddressDetails(LoggedUserModel loggedUserModel);
    }
}
