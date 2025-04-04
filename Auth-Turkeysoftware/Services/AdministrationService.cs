using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories;

namespace Auth_Turkeysoftware.Services
{
    public class AdministrationService : IAdministrationService
    {
        private readonly IUserSessionRepository _userSessionRepository;

        public AdministrationService(IUserSessionRepository userSessionRepository)
        {
            _userSessionRepository = userSessionRepository;
        }

        public async Task InvalidateUserSession(string userId, string sessionId)
        {
                await _userSessionRepository.InvalidateUserSession(userId, sessionId);
        }

        public async Task InvalidateAllUserSession(string userId)
        {
            await _userSessionRepository.InvalidateAllUserSessions(userId);
        }

        public async Task<PaginationDTO<UserSessionResponse>> ListUserActiveSessions(string userId, int page)
        {
            if (page <= 0)
                page = 1;

            return await _userSessionRepository.ListUserActiveSessionsPaginated(userId, page, 10);
        }
    }
}
