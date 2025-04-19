using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories.Context;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Repositories
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private const string ERROR_UPDATE_DB = "Houve um erro de acesso ao banco de dados durante a atualização da sessão do usuário";
        private const int MAX_RETROACTIVE_DAYS = -7;
        internal AppDbContext _dbContext;
        private readonly ILogger<UserSessionRepository> _logger;

        public UserSessionRepository(AppDbContext dataBaseContext, ILogger<UserSessionRepository> logger)
        {
            this._dbContext = dataBaseContext;
            this._logger = logger;
        }

        public async Task AddLoggedUser(UserSessionModel loggedUser)
        {
            try
            {
                loggedUser.TokenStatus = (char)StatusTokenEnum.ATIVO;
                loggedUser.CreatedOn = DateTime.Now.ToUniversalTime();
                if (!loggedUser.IsValidForInclusion()) {
                    throw new BusinessException("Os campos EmailUsuario, RefreshToken e IP são obrigatórios.");
                }

                _dbContext.LoggedUser.Add(loggedUser);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Houve um erro de acesso ao banco de dados durante a inclusão da sessão do usuário.");
                throw new BusinessException("Não foi possível salvar o registro de login do usuário.");
            }
        }

        public async Task<UserSessionModel?> FindRefreshToken(string userId, string sessionId, string userToken)
        {
            return await _dbContext.LoggedUser
                                  .AsNoTracking()
                                  .Where(p => p.SessionId == sessionId && p.FkUserId == userId
                                              && p.RefreshToken == userToken)
                                  .Select(p => new UserSessionModel
                                  {
                                      SessionId = p.SessionId,
                                      FkUserId = p.FkUserId,
                                      TokenStatus = p.TokenStatus
                                  })
                                  .FirstOrDefaultAsync();
        }

        public async Task InvalidateUserSession(string userId, string sessionId)
        {
            try
            {
                int rowsAffected = await _dbContext.LoggedUser
                                                  .Where(p => p.SessionId == sessionId
                                                           && p.FkUserId == userId)
                                                  .ExecuteUpdateAsync(p => p.SetProperty(e => e.TokenStatus, (char)StatusTokenEnum.INATIVO)
                                                                            .SetProperty(e => e.UpdatedOn, DateTime.Now.ToUniversalTime()));

                if (rowsAffected <= 0) {
                    throw new InvalidSessionException("Não foi possível encontrar a sessão à ser revogada.");
                }
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, ERROR_UPDATE_DB);
                throw new BusinessException("Não foi possível dar update no registro de login do usuário.");
            }
        }

        public async Task UpdateSessionRefreshToken(string userId, string sessionId, string oldRefreshToken, string newRefreshToken)
        {
            try
            {
                int rowsAffected = await _dbContext.LoggedUser
                                                  .Where(p => p.SessionId == sessionId && p.FkUserId == userId
                                                           && p.RefreshToken == oldRefreshToken && p.TokenStatus == (char)StatusTokenEnum.ATIVO)
                                                  .ExecuteUpdateAsync(p => p.SetProperty(e => e.RefreshToken, newRefreshToken)
                                                                            .SetProperty(e => e.UpdatedOn, DateTime.Now.ToUniversalTime()));

                if (rowsAffected <= 0) {
                    throw new InvalidSessionException("Não foi possível encontrar uma sessão válida que esteja utilizando o refresh token informado.");
                }

            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, ERROR_UPDATE_DB);
                throw new BusinessException("Não foi possível dar update no registro de login do usuário.");
            }
        }

        private IQueryable<UserSessionResponse> GetQueryListUserActiveSessionsPaginated(string userId) {

            DateTime currentDate = DateTime.Now.ToUniversalTime();
            DateTime sevenDaysAgo = currentDate.AddDays(MAX_RETROACTIVE_DAYS).ToUniversalTime();

            return _dbContext.LoggedUser
                            .AsNoTracking()
                            .Where(p => p.FkUserId == userId && p.TokenStatus == (char)StatusTokenEnum.ATIVO
                                        && (p.UpdatedOn > sevenDaysAgo || (p.CreatedOn > sevenDaysAgo && p.UpdatedOn == null)))
                            .Select(p => new UserSessionResponse
                            {
                                SessionId = p.SessionId,
                                TokenStatus = p.TokenStatus,
                                CreatedOn = p.CreatedOn,
                                LastTimeOnline = p.UpdatedOn ?? p.CreatedOn,
                                IP = p.IP,
                                Platform = p.Platform,
                                Country = p.Country,
                                UF = p.UF
                            }).OrderByDescending(p => p.CreatedOn);
        }

        public async Task<PaginationDto<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int pageNumber, int pageSize)
        {
            return await _dbContext.GetPagedResultAsync<UserSessionResponse>(GetQueryListUserActiveSessionsPaginated(userId), pageNumber, pageSize);
        }

        public async Task<long> ListUserActiveSessionsCount(IQueryable<UserSessionResponse> query)
        {
            return await query.CountAsync();
        }

        public async Task InvalidateAllUserSessions(string userId)
        {
            await _dbContext.LoggedUser
                           .Where(p => p.FkUserId == userId && p.TokenStatus == (char)StatusTokenEnum.ATIVO)
                           .ExecuteUpdateAsync(p => p.SetProperty(e => e.TokenStatus, (char)StatusTokenEnum.INATIVO));
        }
    }
}
