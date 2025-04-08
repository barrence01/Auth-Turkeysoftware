using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services.ExternalServices;

namespace Auth_Turkeysoftware.Services
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
        public async Task<PaginationDto<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int page)
        {
            if (page <= 0)
                page = 1;

            return await _userSessionRepository.ListUserActiveSessionsPaginated(userId, page, 10);
        }
    }
}
