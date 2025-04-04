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
        private static readonly string ERROR_UPDATE_DB = "Houve um erro de acesso ao banco de dados durante a atualização da sessão do usuário";
        internal AppDbContext dbContext;
        private readonly ILogger<UserSessionRepository> _logger;

        public UserSessionRepository(AppDbContext dataBaseContext, ILogger<UserSessionRepository> logger)
        {
            this.dbContext = dataBaseContext;
            this._logger = logger;
        }

        public async Task AddLoggedUser(UserSessionModel loggedUser)
        {
            try
            {
                loggedUser.TokenStatus = (char)StatusTokenEnum.ATIVO;
                loggedUser.DataInclusao = DateTime.Now.ToUniversalTime();
                if (!loggedUser.IsValidForInclusion()) {
                    throw new BusinessException("Os campos EmailUsuario, RefreshToken e IP são obrigatórios.");
                }

                dbContext.LoggedUser.Add(loggedUser);
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Houve um erro de acesso ao banco de dados durante a inclusão da sessão do usuário.");
                throw new BusinessException("Não foi possível salvar o registro de login do usuário.");
            }
        }

        public async Task<UserSessionModel?> FindRefreshToken(string userId, string sessionId, string userToken)
        {
            return await dbContext.LoggedUser
                                    .AsNoTracking()
                                    .Where(p => p.IdSessao == sessionId && p.FkIdUsuario == userId
                                                && p.RefreshToken == userToken)
                                    .Select(p => new UserSessionModel
                                    {
                                        IdSessao = p.IdSessao,
                                        FkIdUsuario = p.FkIdUsuario,
                                        TokenStatus = p.TokenStatus
                                    })
                                    .FirstOrDefaultAsync();
        }

        public async Task InvalidateUserSession(string userId, string sessionId)
        {
            try
            {
                int rowsAffected = await dbContext.LoggedUser
                                                    .Where(p => p.IdSessao == sessionId
                                                             && p.FkIdUsuario == userId)
                                                    .ExecuteUpdateAsync(p => p.SetProperty(e => e.TokenStatus, (char)StatusTokenEnum.INATIVO)
                                                                              .SetProperty(e => e.DataAlteracao, DateTime.Now.ToUniversalTime()));

                if (rowsAffected <= 0)
                    throw new InvalidSessionException("Não foi possível encontrar a sessão à ser revogada.");
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
                int rowsAffected = await dbContext.LoggedUser
                                                    .Where(p => p.IdSessao == sessionId && p.FkIdUsuario == userId
                                                                && p.RefreshToken == oldRefreshToken && p.TokenStatus == (char)StatusTokenEnum.ATIVO)
                                                    .ExecuteUpdateAsync(p => p.SetProperty(e => e.RefreshToken, newRefreshToken)
                                                                                .SetProperty(e => e.DataAlteracao, DateTime.Now.ToUniversalTime()));

                if (rowsAffected <= 0)
                    throw new InvalidSessionException("Não foi possível encontrar uma sessão válida que esteja utilizando o refresh token informado.");

            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, ERROR_UPDATE_DB);
                throw new BusinessException("Não foi possível dar update no registro de login do usuário.");
            }
        }

        public async Task<PaginationDTO<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int page, int pageSize)
        {
            DateTime currentDate = DateTime.Now.ToUniversalTime();
            DateTime sevenDaysAgo = currentDate.AddDays(-7).ToUniversalTime();

            long rowsCount = await this.ListUserActiveSessionsCount(userId, sevenDaysAgo);
            int pageCount = (int)Math.Ceiling((double)rowsCount / (double)pageSize);

            if (rowsCount <= 0 || page > pageCount) {
                return new PaginationDTO<UserSessionResponse>([], page, pageSize, rowsCount);
            }

            var sessoes = await dbContext.LoggedUser
                                            .AsNoTracking()
                                            .Where(p => p.FkIdUsuario == userId && p.TokenStatus == (char)StatusTokenEnum.ATIVO
                                                     && (p.DataAlteracao > sevenDaysAgo || (p.DataInclusao > sevenDaysAgo && p.DataAlteracao == null)))
                                            .Select(p => new UserSessionResponse
                                            {
                                                IdSessao = p.IdSessao,
                                                TokenStatus = p.TokenStatus,
                                                DataInclusao = p.DataInclusao,
                                                UltimaVezOnline = p.DataAlteracao ?? p.DataInclusao,
                                                IP = p.IP,
                                                Platform = p.Platforma,
                                                Pais = p.Pais,
                                                UF = p.UF
                                            }).OrderByDescending(p => p.DataInclusao)
                                            .Skip((page - 1) * pageSize)
                                            .Take(pageSize)
                                            .ToListAsync();

            return new PaginationDTO<UserSessionResponse>(sessoes, page, pageSize, rowsCount);
        }

        public async Task<long> ListUserActiveSessionsCount(string userId, DateTime minDay)
        {
            return await dbContext.LoggedUser
                                    .AsNoTracking()
                                    .Where(p => p.FkIdUsuario == userId && p.TokenStatus == (char)StatusTokenEnum.ATIVO
                                                && (p.DataAlteracao > minDay || (p.DataInclusao > minDay && p.DataAlteracao == null)))
                                    .OrderByDescending(p => p.DataInclusao)
                                    .CountAsync();
        }

        public async Task InvalidateAllUserSessions(string userId)
        {
            await dbContext.LoggedUser
                            .Where(p => p.FkIdUsuario == userId && p.TokenStatus == (char)StatusTokenEnum.ATIVO)
                            .ExecuteUpdateAsync(p => p.SetProperty(e => e.TokenStatus, (char)StatusTokenEnum.INATIVO));
        }
    }
}
