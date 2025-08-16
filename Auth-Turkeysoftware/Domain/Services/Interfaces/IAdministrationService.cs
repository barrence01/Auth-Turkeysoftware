using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Domain.Models.VOs;

namespace Auth_Turkeysoftware.Domain.Services.Interfaces
{
    public interface IAdministrationService
    {
        Task InvalidateUserSession(Guid userId, Guid sessionId);
        Task InvalidateAllUserSession(Guid userId);
        Task<PaginationVO<UserSessionResponse>> ListUserActiveSessions(Guid userId, int page);
    }
}
