using Auth_Turkeysoftware.Models.DataBaseModels;

namespace Auth_Turkeysoftware.Repositories
{
    public interface ILoggedUserRepository
    {
        Task AddLoggedUser(LoggedUserModel loggedUser);

        Task<LoggedUserModel?> FindBlackListedTokenByUserIdAndToken(string UserId, string UserToken);

        Task RemoveOlderThan30DaysFromBlackList(LoggedUserModel loggedUserModel);

        Task UpdateTokenToBlackList(LoggedUserModel loggedUserModel);

        Task<List<LoggedUserModel>> GetActiveUserSessionsByUserId(string UserId);
    }
}
