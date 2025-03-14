using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Models.DTOs;

namespace Auth_Turkeysoftware.Repositories
{
    public interface IUserSessionRepository
    {
        Task AddLoggedUser(UserSessionModel loggedUser);

        Task<UserSessionModel?> FindRefreshToken(string userId, string sessionId, string userToken);

        Task InvalidateUserSessionByIdSessaoAndIdUsuario(string idSessao, string idUsuario);

        Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string oldRefreshToken, string newRefreshToken);

        Task<List<UserSessionDTO>> GetUserActiveSessionsByUserId(string UserId, int pagina, int qtdRegistrosPorPagina);

        Task<long> GetUserActiveSessionsByUserIdCount(string UserId);
    }
}
