using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories;

namespace Auth_Turkeysoftware.Services
{
    public class AdministrationService : IAdministrationService
    {
        private readonly IAdministrationRepository _administrationRepository;

        private readonly IUserSessionRepository _userSessionRepository;

        public AdministrationService(IAdministrationRepository administrationRepository,
                                     IUserSessionRepository userSessionRepository)
        {
            _administrationRepository = administrationRepository;
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

        public async Task<PaginationDTO<UserSessionResponse>> ListUserActiveSessions(string userId, int pagina)
        {
            if (pagina <= 0)
                pagina = 1;

            return await _userSessionRepository.ListUserActiveSessionsPaginated(userId, pagina, 10);
        }
    }
}
