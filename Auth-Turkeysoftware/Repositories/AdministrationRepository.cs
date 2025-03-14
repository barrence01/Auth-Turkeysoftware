using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Extensions;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

namespace Auth_Turkeysoftware.Repositories
{
    public class AdministrationRepository : IAdministrationRepository
    {
        private static readonly string ERROR_UPDATE_DB = "Houve um erro de acesso ao banco de dados durante a atualização da sessão do usuário";
        internal AppDbContext dataBaseContext;

        public AdministrationRepository(AppDbContext dataBaseContext)
        {
            this.dataBaseContext = dataBaseContext;
        }

        public async Task InvalidateAllUserSessionByEmail(string userId)
        {
            try
            {
                var sessao = await dataBaseContext.LoggedUser
                                              .AsNoTracking()
                                              .Where(p => p.FkIdUsuario == userId)
                                              .Select(p => new UserSessionModel
                                              {
                                                  IdSessao = p.IdSessao
                                              }).FirstOrDefaultAsync();
                if (sessao == null)
                    throw new BusinessRuleException("Não foi possível encontrar a sessão à ser revogada.");

                dataBaseContext.Attach(sessao);

                sessao.TokenStatus = (char)StatusTokenEnum.INATIVO;
                sessao.DataAlteracao = DateTime.Now.ToUniversalTime();

                await dataBaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                Log.Error(e, ERROR_UPDATE_DB);
                throw new BusinessRuleException("Não foi possível dar update no registro de login do usuário.");
            }
        }

        public async Task AddToLog(string username, string methodName, string arguments) {
            var logEntry = new AdminActionLogModel(username, methodName, arguments);
            logEntry.TruncateAllFields();

            dataBaseContext.AdminActionLog.Add(logEntry);
            await dataBaseContext.SaveChangesAsync();
        }
    }
}
