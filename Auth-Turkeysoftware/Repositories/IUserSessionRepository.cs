using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Repositories
{
    public interface IUserSessionRepository
    {
        Task AddLoggedUser(UserSessionModel loggedUser);

        Task<UserSessionModel?> FindRefreshToken(string userId, string sessionId, string userToken);

        Task InvalidateUserSessionByIdSessaoAndIdUsuario(string idSessao, string idUsuario);

        Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string oldRefreshToken, string newRefreshToken);

        Task<PaginationDTO<UserSessionResponse>> GetUserActiveSessionsByUserId(string userId, int pagina, int tamanhoPagina);

        Task<long> GetUserActiveSessionsByUserIdCount(string userId, DateTime dataLimite);
    }
}
