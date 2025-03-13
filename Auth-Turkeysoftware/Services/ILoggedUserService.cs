using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Models.DataBaseModels;
using System.IdentityModel.Tokens.Jwt;

namespace Auth_Turkeysoftware.Services
{
    public interface ILoggedUserService
    {

        Task AddLoggedUser(LoggedUserModel loggedUserModel);

        Task InvalidateUserSession(string idSessao, string idUsuario);

        Task<bool> IsTokenBlackListed(string idUsuario, string idSessao, string userToken);

        Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string refreshToken, string newRefreshToken);

        Task<LoggedUserModel> GetGeolocationByIpAddress(LoggedUserModel loggedUserModel);

        Task<PaginationModel<List<UserSessionModel>>> GetUserActiveSessions(string UserId, int pagina);
    }
}
