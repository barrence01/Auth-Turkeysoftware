using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories;

namespace Auth_Turkeysoftware.Services
{
    public class LoggedUserService : ILoggedUserService
    {
        private readonly ILoggedUserRepository _loggedUserRepository;

        public LoggedUserService(ILoggedUserRepository loggedUserRepository)
        {
            _loggedUserRepository = loggedUserRepository;
        }

        public async Task AddLoggedUser(LoggedUserModel loggedUserModel)
        {
            await _loggedUserRepository.AddLoggedUser(loggedUserModel);
        }

        public async Task AddTokenInBlackList(LoggedUserModel loggedUserModel)
        {
            await _loggedUserRepository.UpdateTokenToBlackList(loggedUserModel);
        }

        public async Task<bool> IsBlackListed(string userId, string UserToken)
        {
            var result = await _loggedUserRepository.FindBlackListedTokenByUserIdAndToken(userId, UserToken);
            if (result != null)
                return true;

            return false;
        }
    }
}
