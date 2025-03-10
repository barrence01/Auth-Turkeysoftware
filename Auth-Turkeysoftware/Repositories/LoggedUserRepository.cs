using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Auth_Turkeysoftware.Repositories
{
    public class LoggedUserRepository : ILoggedUserRepository
    {
        private static readonly string ERROR_UPDATE_DB = "Houve um erro de acesso ao banco de dados durante a atualização da sessão do usuário";
        internal AppDbContext dataBaseContext;

        public LoggedUserRepository(AppDbContext dataBaseContext)
        {
            this.dataBaseContext = dataBaseContext;
        }

        public async Task AddLoggedUser(LoggedUserModel loggedUser)
        {
            try
            {
                loggedUser.TokenStatus = (char)StatusTokenEnum.ATIVO;
                loggedUser.DataInclusao = DateTime.Now;
                if (!loggedUser.IsValidForInclusion())
                {
                    throw new BusinessRuleException("Os campos EmailUsuario, RefreshToken e IP são obrigatórios.");
                }

                dataBaseContext.LoggedUser.Add(loggedUser);
                await dataBaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                Log.Error(e, "Houve um erro de acesso ao banco de dados durante a inclusão da sessão do usuário.");
                throw new BusinessRuleException("Não foi possível salvar o registro de login do usuário.");
            }
        }

        public async Task<LoggedUserModel?> FindRefreshToken(string userId, string sessionId, string userToken)
        {
            return await dataBaseContext.LoggedUser
                                        .AsNoTracking()
                                        .Where(p => p.IdSessao == sessionId && p.FkIdUsuario == userId
                                                 && p.RefreshToken == userToken)
                                        .Select(p => new LoggedUserModel
                                        {
                                            IdSessao = p.IdSessao
                                        })
                                        .FirstOrDefaultAsync();
        }

        public async Task InvalidateUserSessionByIdSessaoAndIdUsuario(string idSessao, string idUsuario)
        {
            try
            {
                var sessao = await dataBaseContext.LoggedUser
                                              .AsNoTracking()
                                              .Where(p => p.IdSessao == idSessao
                                                       && p.FkIdUsuario == idUsuario)
                                              .Select(p => new LoggedUserModel
                                              {
                                                  IdSessao = p.IdSessao
                                              }).FirstOrDefaultAsync();
                if (sessao == null)
                    throw new BusinessRuleException("Não foi possível encontrar a sessão à ser revogada.");

                dataBaseContext.Attach(sessao); // Reseta colunas para 'não alterada' no contexto

                sessao.TokenStatus = (char)StatusTokenEnum.INATIVO;
                sessao.DataAlteracao = DateTime.Now;

                await dataBaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                Log.Error(e, ERROR_UPDATE_DB);
                throw new BusinessRuleException("Não foi possível dar update no registro de login do usuário.");
            }
        }

        public async Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string oldRefreshToken, string newRefreshToken)
        {
            try
            {
                var sessao = await dataBaseContext.LoggedUser
                                              .AsNoTracking()
                                              .Where(p => p.IdSessao == idSessao
                                                       && p.FkIdUsuario == idUsuario
                                                       && p.RefreshToken == oldRefreshToken
                                                       && p.TokenStatus == (char)StatusTokenEnum.ATIVO)
                                              .Select(p => new LoggedUserModel
                                              {
                                                  IdSessao = p.IdSessao
                                              }).FirstOrDefaultAsync();
                if (sessao == null)
                    throw new InvalidSessionException("Não foi possível encontrar uma sessão válida que esteja utilizando o refresh token informado.");

                // Reseta colunas para não alterada no contexto
                dataBaseContext.Attach(sessao);
                sessao.DataAlteracao = DateTime.Now;
                sessao.RefreshToken = newRefreshToken;

                await dataBaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                Log.Error(e, ERROR_UPDATE_DB);
                throw new BusinessRuleException("Não foi possível dar update no registro de login do usuário.");
            }
        }

        public async Task<List<UserSessionModel>> GetUserActiveSessionsByUserId(string UserId, int pagina, int qtdRegistrosPorPagina)
        {
            DateTime dataAtual = DateTime.Now;
            DateTime dataLimite = dataAtual.AddDays(-7);

            return await dataBaseContext.LoggedUser
                                        .AsNoTracking()
                                        .Where(p => p.FkIdUsuario == UserId && p.TokenStatus == (char)StatusTokenEnum.ATIVO
                                                 && (p.DataAlteracao > dataLimite || (p.DataInclusao > dataLimite && p.DataAlteracao == null)))
                                        .Select(p => new UserSessionModel
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
                                        .Skip((pagina - 1) * qtdRegistrosPorPagina)
                                        .Take(qtdRegistrosPorPagina)
                                        .ToListAsync();
        }

        public async Task<long> GetUserActiveSessionsByUserIdCount(string UserId)
        {
            DateTime dataAtual = DateTime.Now;
            DateTime dataLimite = dataAtual.AddDays(-7);

            return await dataBaseContext.LoggedUser
                                        .AsNoTracking()
                                        .Where(p => p.FkIdUsuario == UserId && p.TokenStatus == (char)StatusTokenEnum.ATIVO
                                                 && (p.DataAlteracao > dataLimite || (p.DataInclusao > dataLimite && p.DataAlteracao == null)))
                                        .OrderByDescending(p => p.DataInclusao)
                                        .CountAsync();
        }
    }
}
