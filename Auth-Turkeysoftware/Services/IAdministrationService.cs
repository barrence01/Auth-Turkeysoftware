using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;

namespace Auth_Turkeysoftware.Services
{
    public interface IAdministrationService
    {
        Task InvalidateUserSession(string userId, string sessionId);
        Task InvalidateAllUserSession(string userId);
        Task<PaginationDto<UserSessionResponse>> ListUserActiveSessions(string userId, int page);
    }
}
