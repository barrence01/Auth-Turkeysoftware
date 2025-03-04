using Auth_Turkeysoftware.Models.DataBaseModels;

namespace Auth_Turkeysoftware.Repositories
{
    public interface ILoggedUserRepository
    {
        Task AddLoggedUser(LoggedUserModel loggedUser);

        Task<LoggedUserModel?> FindBlackListedTokenByUserIdAndToken(string UserId, string UserToken);

        Task UpdateTokenToBlackListByIdAndIdUsuario(int idSessao, string idUsuario);

        Task UpdateSessionRefreshToken(string idUsuario, string oldRefreshToken, string newRefreshToken);

        Task RemoveRecordsOlderThan30Days(LoggedUserModel userLoggedUserModel);

        Task<List<LoggedUserModel>> GetActiveUserSessionsByUserId(string UserId);

        Task LongRunningQuery(int seconds);
    }
}
