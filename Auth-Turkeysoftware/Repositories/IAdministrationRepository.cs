namespace Auth_Turkeysoftware.Repositories
{
    public interface IAdministrationRepository
    {
        Task InvalidateAllUserSessionByEmail(string userId);
        Task AddToLog(string username, string methodName, string arguments);
    }
}
