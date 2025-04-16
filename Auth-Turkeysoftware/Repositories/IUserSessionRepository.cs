using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Repositories
{
    public interface IUserSessionRepository
    {
        Task AddLoggedUser(UserSessionModel loggedUser);

        Task<UserSessionModel?> FindRefreshToken(string userId, string sessionId, string userToken);

        Task InvalidateUserSession(string userId, string sessionId);

        Task UpdateSessionRefreshToken(string userId, string sessionId, string oldRefreshToken, string newRefreshToken);

        Task<PaginationDto<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int pageNumber, int pageSize);

        Task<long> ListUserActiveSessionsCount(IQueryable<UserSessionResponse> query);

        Task InvalidateAllUserSessions(string userId);
    }
}
