using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Domain.Models.VOs;

namespace Auth_Turkeysoftware.Domain.Services.Interfaces
{
    public interface IAdministrationService
    {
        Task InvalidateUserSession(string userId, string sessionId);
        Task InvalidateAllUserSession(string userId);
        Task<PaginationVO<UserSessionResponse>> ListUserActiveSessions(string userId, int page);
    }
}
