using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
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
                Log.Error($"Houve um erro de acesso ao banco de dados durante a inclusão da sessão do usuário:\n", e);
                throw new BusinessRuleException("Não foi possível salvar o registro de login do usuário.");
            }
        }

        public async Task<LoggedUserModel?> FindBlackListedTokenByUserIdAndToken(string UserId, string UserToken)
        {
            return await dataBaseContext.LoggedUser
                                        .AsNoTracking()
                                        .Where(p => p.FkIdUsuario == UserId && p.RefreshToken == UserToken && p.TokenStatus == (char)StatusTokenEnum.INATIVO)
                                        .Select(p => new LoggedUserModel
                                        {
                                            IdSessao = p.IdSessao
                                        })
                                        .FirstOrDefaultAsync();
        }

        public async Task UpdateTokenToBlackListByIdAndIdUsuario(int idSessao, string idUsuario)
        {
            try
            {
                var sessao = await dataBaseContext.LoggedUser
                                              .Where(p => p.IdSessao == idSessao
                                                       && p.FkIdUsuario == idUsuario)
                                              .Select(p => new LoggedUserModel
                                              {
                                                  IdSessao = p.IdSessao
                                              }).FirstOrDefaultAsync();
                if (sessao == null)
                    throw new BusinessRuleException("Não foi possível encontrar a sessão à ser revogada.");

                dataBaseContext.Attach(sessao); // Reseta colunas para não alterada no contexto

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

        public async Task UpdateSessionRefreshToken(string idUsuario, string oldRefreshToken, string newRefreshToken)
        {
            try
            {
                var sessao = await dataBaseContext.LoggedUser
                                              .Where(p => p.FkIdUsuario == idUsuario
                                                       && p.RefreshToken == oldRefreshToken
                                                       && p.TokenStatus == (char)StatusTokenEnum.ATIVO)
                                              .Select(p => new LoggedUserModel
                                              {
                                                  IdSessao = p.IdSessao
                                              }).FirstOrDefaultAsync();
                if (sessao == null)
                    throw new BusinessRuleException("Não foi possível encontrar uma sessão válida que esteja utilizando o refresh token informado.");

                dataBaseContext.Attach(sessao); // Reseta colunas para não alterada no contexto
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

        public async Task RemoveRecordsOlderThan30Days(LoggedUserModel userLoggedUserModel)
        {
            DateTime dataAtual = DateTime.Now;
            DateTime dataLimite = dataAtual.AddDays(-30);
            var itensParaRemover = await dataBaseContext.LoggedUser
                                                        .Where(p => p.DataAlteracao < dataLimite)
                                                        .ToArrayAsync();
            dataBaseContext.LoggedUser.RemoveRange(itensParaRemover);
            await dataBaseContext.SaveChangesAsync();
        }

        public async Task<List<LoggedUserModel>> GetActiveUserSessionsByUserId(string UserId)
        {
            return await dataBaseContext.LoggedUser
                                        .Where(p => p.FkIdUsuario == UserId && p.TokenStatus == (char)StatusTokenEnum.ATIVO)
                                        .Select(p => new LoggedUserModel
                                        {
                                            IdSessao = p.IdSessao,
                                            TokenStatus = p.TokenStatus,
                                            DataInclusao = p.DataInclusao,
                                            Pais = p.Pais,
                                            UF = p.UF,
                                            IP = p.IP
                                        }).OrderByDescending(p => p.DataInclusao)
                                        .ToListAsync();
        }

        public async Task LongRunningQuery(int seconds)
        {
            Log.Error("Antes do metódo");
            var pSeconds = new MySqlParameter("@seconds", seconds);
            await dataBaseContext.Database.ExecuteSqlRawAsync("SELECT SLEEP(@seconds)", pSeconds);
            //await dataBaseContext.Database.CloseConnectionAsync();
            Log.Error("Depois do método");
        }
    }
}
