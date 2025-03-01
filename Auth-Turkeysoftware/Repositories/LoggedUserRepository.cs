using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Auth_Turkeysoftware.Repositories
{
    public class LoggedUserRepository : ILoggedUserRepository
    {
        internal AppDbContext dataBaseContext;

        public LoggedUserRepository(AppDbContext dataBaseContext)
        {
            this.dataBaseContext = dataBaseContext;
        }

        public async Task AddLoggedUser(LoggedUserModel loggedUser)
        {
            try
            {
                loggedUser.TokenStatus = 'A';
                loggedUser.DataInclusao = DateTime.Now;
                if (!loggedUser.isValidForInclusion())
                {
                    throw new BusinessRuleException("Os campos EmailUsuario, RefreshToken e IP são obrigatórios.");
                }

                dataBaseContext.LoggedUser.Add(loggedUser);
                await dataBaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                Log.Error($"Houve um erro de acesso ao banco de dados durante a inclusão da sessão do usuário{e.Message}");
                throw new BusinessRuleException("Não foi possível salvar o registro de login do usuário.");
            }
        }

        public async Task<LoggedUserModel?> FindBlackListedTokenByUserIdAndToken(string UserId, string UserToken)
        {
            return await dataBaseContext.LoggedUser
                                  .Where(p => p.FkIdUsuario == UserId && p.RefreshToken == UserToken && p.TokenStatus == 'X')
                                  .AsNoTracking()
                                  .Select(p => new LoggedUserModel
                                  {
                                      EmailUsuario = p.EmailUsuario,
                                      RefreshToken = p.RefreshToken
                                  })
                                  .FirstOrDefaultAsync();
        }

        public async Task UpdateTokenToBlackList(LoggedUserModel loggedUserModel)
        {
            loggedUserModel.TokenStatus = 'X';
            loggedUserModel.DataAlteracao = DateTime.Now;
            var sessao = await dataBaseContext.LoggedUser
                                              .Where(p => p.IdSessao == loggedUserModel.IdSessao
                                                       && p.FkIdUsuario == loggedUserModel.FkIdUsuario)
                                              .Select(p => new LoggedUserModel
                                              {
                                                  IdSessao = p.IdSessao
                                              }).FirstOrDefaultAsync();
            if (sessao == null)
                throw new BusinessRuleException("Não foi possível encontrar a sessão para ser revogada.");

            dataBaseContext.Attach(loggedUserModel);
            dataBaseContext.Entry(loggedUserModel).Property(nameof(loggedUserModel.TokenStatus)).IsModified = true;
            dataBaseContext.Entry(loggedUserModel).Property(nameof(loggedUserModel.DataAlteracao)).IsModified = true;
            await dataBaseContext.SaveChangesAsync();
        }

        public async Task RemoveOlderThan30DaysFromBlackList(LoggedUserModel userLoggedUserModel)
        {
            DateTime dataAtual = DateTime.Now;
            DateTime dataLimite = dataAtual.AddDays(-30);
            var itensParaRemover = await dataBaseContext.LoggedUser
                .Where(p => p.DataInclusao < dataLimite && p.TokenStatus == 'X')
                .ToArrayAsync();
            dataBaseContext.LoggedUser.RemoveRange(itensParaRemover);
            await dataBaseContext.SaveChangesAsync();
        }

        public async Task<List<LoggedUserModel>> GetActiveUserSessionsByUserId(string UserId)
        {
            return await dataBaseContext.LoggedUser
                            .Where(p => p.FkIdUsuario == UserId)
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
    }
}
