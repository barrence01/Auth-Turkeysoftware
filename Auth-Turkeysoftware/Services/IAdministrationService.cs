using Auth_Turkeysoftware.Models.DTOs;

namespace Auth_Turkeysoftware.Services
{
    public interface IAdministrationService
    {
        Task InvalidateUserSession(string userId, string userSessionId);
        Task<PaginationDTO<List<UserSessionDTO>>> GetUserAllSessions(string userId, int pagina);
    }
}
