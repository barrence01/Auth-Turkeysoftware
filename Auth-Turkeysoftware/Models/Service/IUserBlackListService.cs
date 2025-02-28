namespace Auth_Turkeysoftware.Models.Facade
{
    public interface IUserBlackListService
    {
        Task AddTokenInBlackList(UserBlackListModel userBlackListModel);

        Task<bool> IsBlackListed(string UserEmail, string UserToken);
    }
}
