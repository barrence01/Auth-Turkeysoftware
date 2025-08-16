using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Domain.Models.VOs;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Interfaces
{
    public interface IUserSessionRepository
    {
        Task AddLoggedUser(UserSessionModel loggedUser);

        Task<UserSessionModel?> FindRefreshToken(Guid userId, Guid sessionId, string userToken);

        Task InvalidateUserSession(Guid userId, Guid sessionId);

        Task UpdateSessionRefreshToken(Guid userId, Guid sessionId, string oldRefreshToken, string newRefreshToken);

        Task<PaginationVO<UserSessionResponse>> ListUserActiveSessionsPaginated(Guid userId, int pageNumber, int pageSize);

        Task<long> ListUserActiveSessionsCount(IQueryable<UserSessionResponse> query);

        Task InvalidateAllUserSessions(Guid userId);
    }
}
