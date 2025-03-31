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
        internal AppDbContext dataBaseContext;
        private readonly ILogger<UserSessionRepository> _logger;

        public UserSessionRepository(AppDbContext dataBaseContext, ILogger<UserSessionRepository> logger)
        {
            this.dataBaseContext = dataBaseContext;
            this._logger = logger;
        }

        public async Task AddLoggedUser(UserSessionModel loggedUser)
        {
            try
            {
                loggedUser.TokenStatus = (char)StatusTokenEnum.ATIVO;
                loggedUser.DataInclusao = DateTime.Now.ToUniversalTime();
                if (!loggedUser.IsValidForInclusion()) {
                    throw new BusinessRuleException("Os campos EmailUsuario, RefreshToken e IP são obrigatórios.");
                }

                dataBaseContext.LoggedUser.Add(loggedUser);
                await dataBaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Houve um erro de acesso ao banco de dados durante a inclusão da sessão do usuário.");
                throw new BusinessRuleException("Não foi possível salvar o registro de login do usuário.");
            }
        }

        public async Task<UserSessionModel?> FindRefreshToken(string userId, string sessionId, string userToken)
        {
            return await dataBaseContext.LoggedUser
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

        public async Task InvalidateUserSession(string idUsuario, string idSessao)
        {
            try
            {
                int rowsAffected = await dataBaseContext.LoggedUser
                            .Where(p => p.IdSessao == idSessao
                                     && p.FkIdUsuario == idUsuario)
                            .ExecuteUpdateAsync(p => p.SetProperty(e => e.TokenStatus, (char)StatusTokenEnum.INATIVO)
                                                      .SetProperty(e => e.DataAlteracao, DateTime.Now.ToUniversalTime()));

                if (rowsAffected <= 0)
                    throw new InvalidSessionException("Não foi possível encontrar a sessão à ser revogada.");
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, ERROR_UPDATE_DB);
                throw new BusinessRuleException("Não foi possível dar update no registro de login do usuário.");
            }
        }

        public async Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string oldRefreshToken, string newRefreshToken)
        {
            try
            {
                int rowsAffected = await dataBaseContext.LoggedUser
                                            .Where(p => p.IdSessao == idSessao && p.FkIdUsuario == idUsuario
                                                     && p.RefreshToken == oldRefreshToken && p.TokenStatus == (char)StatusTokenEnum.ATIVO)
                                            .ExecuteUpdateAsync(p => p.SetProperty(e => e.RefreshToken, newRefreshToken)
                                                                      .SetProperty(e => e.DataAlteracao, DateTime.Now.ToUniversalTime()));

                if (rowsAffected <= 0)
                    throw new InvalidSessionException("Não foi possível encontrar uma sessão válida que esteja utilizando o refresh token informado.");

            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, ERROR_UPDATE_DB);
                throw new BusinessRuleException("Não foi possível dar update no registro de login do usuário.");
            }
        }

        public async Task<PaginationDTO<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int pagina, int tamanhoPagina)
        {
            DateTime dataAtual = DateTime.Now.ToUniversalTime();
            DateTime dataLimite = dataAtual.AddDays(-7).ToUniversalTime();

            long qtdRegistros = await this.ListUserActiveSessionsCount(userId, dataLimite);
            int totalPaginas = (int)Math.Ceiling((double)qtdRegistros / (double)tamanhoPagina);

            if (qtdRegistros <= 0 || pagina >= totalPaginas) {
                return new PaginationDTO<UserSessionResponse>([], pagina, tamanhoPagina, qtdRegistros);
            }

            var sessoes = await dataBaseContext.LoggedUser
                                        .AsNoTracking()
                                        .Where(p => p.FkIdUsuario == userId && p.TokenStatus == (char)StatusTokenEnum.ATIVO
                                                 && (p.DataAlteracao > dataLimite || (p.DataInclusao > dataLimite && p.DataAlteracao == null)))
                                        .Select(p => new UserSessionResponse
                                        {
                                            IdSessao = p.IdSessao,
                                            TokenStatus = p.TokenStatus,
                                            DataInclusao = p.DataInclusao,
                                            UltimaVezOnline = p.DataAlteracao ?? p.DataInclusao,
                                            IP = p.IP,
                                            Platform = p.Platform,
                                            Pais = p.Pais,
                                            UF = p.UF
                                        }).OrderByDescending(p => p.DataInclusao)
                                        .Skip((pagina - 1) * tamanhoPagina)
                                        .Take(tamanhoPagina)
                                        .ToListAsync();

            return new PaginationDTO<UserSessionResponse>(sessoes, pagina, tamanhoPagina, qtdRegistros);
        }

        public async Task<long> ListUserActiveSessionsCount(string userId, DateTime dataLimite)
        {
            return await dataBaseContext.LoggedUser
                                        .AsNoTracking()
                                        .Where(p => p.FkIdUsuario == userId && p.TokenStatus == (char)StatusTokenEnum.ATIVO
                                                 && (p.DataAlteracao > dataLimite || (p.DataInclusao > dataLimite && p.DataAlteracao == null)))
                                        .OrderByDescending(p => p.DataInclusao)
                                        .CountAsync();
        }

        public async Task InvalidateAllUserSessions(string userId)
        {
            await dataBaseContext.LoggedUser
                                 .Where(p => p.FkIdUsuario == userId && p.TokenStatus == (char)StatusTokenEnum.ATIVO)
                                 .ExecuteUpdateAsync(p => p.SetProperty(e => e.TokenStatus, (char)StatusTokenEnum.INATIVO));
        }
    }
}
