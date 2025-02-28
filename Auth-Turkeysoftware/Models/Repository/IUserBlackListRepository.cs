namespace Auth_Turkeysoftware.Models.Repository
{
    public interface IUserBlackListRepository
    {
        Task AddTokenInBlackList(UserBlackListModel userBlackListModel);

        Task<UserBlackListModel?> FindBlackListedItemByEmailAndToken(string UserEmail, string UserToken);

        void RemoveOlderThan30DaysFromBlackList(UserBlackListModel userBlackListModel);
    }
}
