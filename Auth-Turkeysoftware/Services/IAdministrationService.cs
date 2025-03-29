using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;

namespace Auth_Turkeysoftware.Services
{
    public interface IAdministrationService
    {
        Task InvalidateUserSession(string userId, string userSessionId);
        Task<PaginationDTO<UserSessionResponse>> GetUserActiveSessions(string userId, int pagina);
    }
}
