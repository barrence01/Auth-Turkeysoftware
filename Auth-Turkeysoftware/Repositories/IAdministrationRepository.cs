using Auth_Turkeysoftware.Models.DataBaseModels;

namespace Auth_Turkeysoftware.Repositories
{
    public interface IAdministrationRepository
    {
        Task InvalidateAllUserSessionByEmail(string userId);
    }
}
