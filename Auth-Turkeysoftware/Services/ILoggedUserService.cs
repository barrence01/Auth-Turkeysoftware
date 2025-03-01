using Auth_Turkeysoftware.Models.DataBaseModels;

namespace Auth_Turkeysoftware.Services
{
    public interface ILoggedUserService
    {
        Task AddLoggedUser(LoggedUserModel loggedUserModel);

        Task AddTokenInBlackList(LoggedUserModel loggedUserModel);

        Task<bool> IsBlackListed(string userId, string UserToken);
    }
}
