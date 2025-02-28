using Auth_Turkeysoftware.Models.Repository;

namespace Auth_Turkeysoftware.Models.Facade
{
    public class UserBlackListFacade : IUserBlackListService
    {
        private readonly IUserBlackListRepository _userBlackListRepository;

        public async Task AddTokenInBlackList(UserBlackListModel userBlackListModel)
        {
            await _userBlackListRepository.AddTokenInBlackList(userBlackListModel);
        }

        public async Task<bool> IsBlackListed(string UserEmail, string UserToken)
        {
            var result = await _userBlackListRepository.FindBlackListedItemByEmailAndToken(UserEmail, UserToken);
            if (result != null)
                return true;

            return false;
        }
    }
}
