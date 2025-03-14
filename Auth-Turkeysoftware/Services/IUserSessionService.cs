using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Models.DTOs;

namespace Auth_Turkeysoftware.Services
{
    public interface IUserSessionService
    {

        Task AddLoggedUser(UserSessionModel loggedUserModel);

        Task InvalidateUserSession(string idSessao, string idUsuario);

        Task<bool> IsTokenBlackListed(string idUsuario, string idSessao, string userToken);

        Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string refreshToken, string newRefreshToken);

        Task<UserSessionModel> GetGeolocationByIpAddress(UserSessionModel loggedUserModel);

        Task<PaginationDTO<List<UserSessionDTO>>> GetUserActiveSessions(string UserId, int pagina);
    }
}
