using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.Repository.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Auth_Turkeysoftware.Models.Repository
{
    public class UserBlackListRepository : IUserBlackListRepository
    {
        internal AppDbContext dataBaseContext;

        public UserBlackListRepository(AppDbContext dataBaseContext)
        {
            this.dataBaseContext = dataBaseContext;
        }

        public async Task AddTokenInBlackList(UserBlackListModel userBlackListModel)
        {
            if (userBlackListModel.EmailUsuario == null || userBlackListModel.RefreshToken == null)
            {
                throw new BusinessRuleException("Os campos EmailUsuario, RefreshToken e DataInclusao são obrigatórios.");
            }

            userBlackListModel.DataInclusao = DateTime.Now;

            dataBaseContext.UserBlackList.Add(userBlackListModel);
            await dataBaseContext.SaveChangesAsync();
        }

        public async Task<UserBlackListModel?> FindBlackListedItemByEmailAndToken(string UserEmail, string UserToken)
        {
            return await dataBaseContext.UserBlackList
                                  .Where(p => p.EmailUsuario == UserEmail && p.RefreshToken == UserToken)
                                  .AsNoTracking()
                                  .Select(p => new UserBlackListModel
                                  {
                                      EmailUsuario = p.EmailUsuario,
                                      RefreshToken = p.RefreshToken
                                  })
                                  .FirstOrDefaultAsync();
        }

        public async void RemoveOlderThan30DaysFromBlackList(UserBlackListModel userBlackListModel)
        {
            DateTime dataAtual = DateTime.Now;
            DateTime dataLimite = dataAtual.AddDays(-30);
            var itensParaRemover = await dataBaseContext.UserBlackList
                .Where(p => p.DataInclusao < dataLimite)
                .ToArrayAsync();
            dataBaseContext.UserBlackList.RemoveRange(itensParaRemover);
            await dataBaseContext.SaveChangesAsync();
        }
    }
}
