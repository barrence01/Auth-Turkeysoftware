using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Repositories
{
    public class AdministrationRepository : IAdministrationRepository
    {
        private static readonly string ERROR_UPDATE_DB = "Houve um erro de acesso ao banco de dados durante a atualização da sessão do usuário";
        internal AppDbContext dataBaseContext;
        private readonly ILogger<AdministrationRepository> _logger;

        public AdministrationRepository(AppDbContext dataBaseContext, ILogger<AdministrationRepository> logger)
        {
            this.dataBaseContext = dataBaseContext;
            this._logger = logger;
        }
    }
}
