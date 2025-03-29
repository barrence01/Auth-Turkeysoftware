using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Repositories
{
    public interface IUserSessionRepository
    {
        Task AddLoggedUser(UserSessionModel loggedUser);

        Task<UserSessionModel?> FindRefreshToken(string userId, string sessionId, string userToken);

        Task InvalidateUserSession(string idUsuario, string idSessao);

        Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string oldRefreshToken, string newRefreshToken);

        Task<PaginationDTO<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int pagina, int tamanhoPagina);

        Task<long> ListUserActiveSessionsCount(string userId, DateTime dataLimite);

        Task InvalidateAllUserSessions(string userId);
    }
}
