using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Services
{
    public interface IUserSessionService
    {

        Task AddLoggedUser(UserSessionModel loggedUserModel);

        Task InvalidateUserSession(string idSessao, string idUsuario);

        Task<bool> IsTokenBlackListed(string idUsuario, string idSessao, string userToken);

        Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string refreshToken, string newRefreshToken);

        Task<UserSessionModel> GetGeolocationByIpAddress(UserSessionModel loggedUserModel);

        Task<PaginationDTO<UserSessionResponse>> GetUserActiveSessions(string userId, int pagina);
    }
}
