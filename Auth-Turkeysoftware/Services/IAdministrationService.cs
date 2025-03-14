namespace Auth_Turkeysoftware.Services
{
    public interface IAdministrationService
    {
        Task InvalidateAllUserSession(string userId);
    }
}
