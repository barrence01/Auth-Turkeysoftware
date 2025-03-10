using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Repositories
{
    public class ManutencaoJOBsRepository
    {
        internal AppDbContext dataBaseContext;

        public ManutencaoJOBsRepository(AppDbContext dataBaseContext)
        {
            this.dataBaseContext = dataBaseContext;
        }

        /// <summary>
        /// Remove registros mais antigos que 7 dias da tabela de sessões
        /// </summary>
        /// <param name="userLoggedUserModel"></param>
        /// <returns></returns>
        public async Task RemoveRecordsOlderThan7Days(LoggedUserModel userLoggedUserModel)
        {
            DateTime dataAtual = DateTime.Now;
            DateTime dataLimite = dataAtual.AddDays(-7);
            var itensParaRemover = await dataBaseContext.LoggedUser
                                                        .Where(p => p.DataAlteracao < dataLimite
                                                                 || (p.DataInclusao < dataLimite && p.DataAlteracao == null))
                                                        .ToArrayAsync();
            dataBaseContext.LoggedUser.RemoveRange(itensParaRemover);
            await dataBaseContext.SaveChangesAsync();
        }
    }
}
