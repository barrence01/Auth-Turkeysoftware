using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Interfaces;
using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Shared.Enums;
using Auth_Turkeysoftware.Domain.Models.VOs;
using Auth_Turkeysoftware.Domain.Services.Interfaces;

namespace Auth_Turkeysoftware.Domain.Services.Implementations
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IUserSessionRepository _userSessionRepository;

        public UserSessionService(IUserSessionRepository loggedUserRepository)
        {
            _userSessionRepository = loggedUserRepository;
        }

        /// <inheritdoc/>
        public async Task AddLoggedUser(UserSessionModel loggedUserModel)
        {
            await _userSessionRepository.AddLoggedUser(loggedUserModel);
        }

        /// <inheritdoc/>
        public async Task InvalidateUserSession(string userId, string sessionId)
        {
            await _userSessionRepository.InvalidateUserSession(userId, sessionId);
        }

        /// <inheritdoc/>
        public async Task InvalidateAllUserSession(string userId, string sessionId)
        {
            await _userSessionRepository.InvalidateUserSession(userId, sessionId);
        }

        /// <inheritdoc/>
        public async Task<bool> IsTokenBlackListed(string userId, string sessionId, string userToken)
        {
            var result = await _userSessionRepository.FindRefreshToken(userId, sessionId, userToken);
            if (result == null || result.TokenStatus == (char)StatusTokenEnum.INATIVO)
                return true;

            return false;
        }

        /// <inheritdoc/>
        public async Task UpdateSessionRefreshToken(string userId, string sessionId, string refreshToken, string newRefreshToken)
        {
            await _userSessionRepository.UpdateSessionRefreshToken(userId, sessionId, refreshToken, newRefreshToken);
        }

        /// <inheritdoc/>
        public async Task<PaginationVO<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int pageNumber)
        {
            if (pageNumber <= 0)
                pageNumber = 1;

            return await _userSessionRepository.ListUserActiveSessionsPaginated(userId, pageNumber, 10);
        }
    }
}
