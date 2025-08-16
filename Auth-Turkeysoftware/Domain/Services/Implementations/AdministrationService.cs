using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Interfaces;
using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Domain.Models.VOs;
using Auth_Turkeysoftware.Domain.Services.Interfaces;

namespace Auth_Turkeysoftware.Domain.Services.Implementations
{
    public class AdministrationService : IAdministrationService
    {
        private readonly IUserSessionRepository _userSessionRepository;

        public AdministrationService(IUserSessionRepository userSessionRepository)
        {
            _userSessionRepository = userSessionRepository;
        }

        public async Task InvalidateUserSession(Guid userId, Guid sessionId)
        {
            await _userSessionRepository.InvalidateUserSession(userId, sessionId);
        }

        public async Task InvalidateAllUserSession(Guid userId)
        {
            await _userSessionRepository.InvalidateAllUserSessions(userId);
        }

        public async Task<PaginationVO<UserSessionResponse>> ListUserActiveSessions(Guid userId, int page)
        {
            if (page <= 0)
                page = 1;

            return await _userSessionRepository.ListUserActiveSessionsPaginated(userId, page, 10);
        }
    }
}
