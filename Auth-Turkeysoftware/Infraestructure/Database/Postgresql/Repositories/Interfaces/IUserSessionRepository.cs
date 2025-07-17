using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Domain.Models.VOs;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Interfaces
{
    public interface IUserSessionRepository
    {
        Task AddLoggedUser(UserSessionModel loggedUser);

        Task<UserSessionModel?> FindRefreshToken(string userId, string sessionId, string userToken);

        Task InvalidateUserSession(string userId, string sessionId);

        Task UpdateSessionRefreshToken(string userId, string sessionId, string oldRefreshToken, string newRefreshToken);

        Task<PaginationVO<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int pageNumber, int pageSize);

        Task<long> ListUserActiveSessionsCount(IQueryable<UserSessionResponse> query);

        Task InvalidateAllUserSessions(string userId);
    }
}
