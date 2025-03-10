using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Models.DataBaseModels;

namespace Auth_Turkeysoftware.Repositories
{
    public interface ILoggedUserRepository
    {
        Task AddLoggedUser(LoggedUserModel loggedUser);

        Task<LoggedUserModel?> FindRefreshToken(string userId, string sessionId, string userToken);

        Task InvalidateUserSessionByIdSessaoAndIdUsuario(string idSessao, string idUsuario);

        Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string oldRefreshToken, string newRefreshToken);

        Task<List<UserSessionModel>> GetUserActiveSessionsByUserId(string UserId, int pagina, int qtdRegistrosPorPagina);

        Task<long> GetUserActiveSessionsByUserIdCount(string UserId);
    }
}
