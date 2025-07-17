using Auth_Turkeysoftware.Shared.Exceptions;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.DbContext;
using Auth_Turkeysoftware.Test.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Test.Repositories
{
    public class TestDataRepository : ITestDataRepository
    {
        internal AppDbContext dataBaseContext;
        private readonly ILogger<TestDataRepository> _logger;

        public TestDataRepository(AppDbContext dataBaseContext, ILogger<TestDataRepository> logger)
        {
            this.dataBaseContext = dataBaseContext;
            _logger = logger;
        }

        public async Task AddData()
        {
            try
            {
                for (var i = 0; i < 10; i++)
                {
                    await dataBaseContext.TestData.AddAsync(new TestDataModel());
                }
                await dataBaseContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Houve um erro de acesso ao banco de dados durante a inclusão da sessão do usuário.");
                throw new BusinessException("Não foi possível salvar o registro de login do usuário.");
            }
        }

        public async Task<List<TestDataModel>> ReadData()
        {
            try
            {
                return await dataBaseContext.TestData
                                                .AsNoTracking()
                                                .Where(p => int.Parse(p.dsString) == 547)
                                                .ToListAsync();
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e, "Houve um erro de acesso ao banco de dados durante a inclusão da sessão do usuário.");
                throw new BusinessException("Não foi possível salvar o registro de login do usuário.");
            }
        }
    }
}
