using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Repositories.Context;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Microsoft.EntityFrameworkCore;

namespace Auth_Turkeysoftware.Repositories
{
    public class TwoFactorRepository : ITwoFactorRepository
    {
        private const string ERROR_ACESSO_DB = "Houve um erro de acesso ao banco de dados.";
        private const string ERROR_INVALID_FOR_INCLUSION = "O objeto não é valido para inclusão";
        internal AppDbContext _dbContext;
        private readonly ILogger<TwoFactorRepository> _logger;

        public TwoFactorRepository(AppDbContext dataBaseContext, ILogger<TwoFactorRepository> logger)
        {
            this._dbContext = dataBaseContext;
            this._logger = logger;
        }

        public async Task AddTwoFactorAuth(TwoFactorAuthModel model)
        {

            if (!model.IsValidForInclusion()) {
                throw new BusinessException(ERROR_INVALID_FOR_INCLUSION);
            }

            try
            {
                model.CreatedOn = DateTime.UtcNow.ToUniversalTime();
                model.IsActive = true;

                await _dbContext.AddAsync(model);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Houve um erro ao incluir o metodo de autenticação de 2 fatores");
                throw new BusinessException("Houve um erro ao incluir o metodo de autenticação de 2 fatores");
            }
        }

        public async Task<TwoFactorAuthModel?> FindByUserIdAndModeAsync(string userId, int twoFactorMode)
        {
            try
            {
                return await _dbContext.TwoFactorAuth.Where(w => w.FkUserId == userId && w.TwoFactorMode == twoFactorMode)
                                                    .FirstOrDefaultAsync();      
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, ERROR_ACESSO_DB);
                throw new BusinessException(ERROR_ACESSO_DB);
            }
        }

        public async Task<List<TwoFactorAuthModel>> ListAll2FAOptionsAsync(string userId)
        {
            try
            {
                return await _dbContext.TwoFactorAuth.Where(w => w.FkUserId == userId)
                                                    .ToListAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, ERROR_ACESSO_DB);
                throw new BusinessException(ERROR_ACESSO_DB);
            }
        }

        public async Task<List<TwoFactorAuthModel>> ListActive2FAOptionsAsync(string userId)
        {
            try
            {
                return await _dbContext.TwoFactorAuth.Where(w => w.FkUserId == userId && w.IsActive == true)
                                                    .ToListAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, ERROR_ACESSO_DB);
                throw new BusinessException(ERROR_ACESSO_DB);
            }
        }

        public async Task UpdateTwoFactorAuth(TwoFactorAuthModel model)
        {
            if (!model.IsValidForInclusion()) {
                throw new BusinessException(ERROR_INVALID_FOR_INCLUSION);
            }

            try
            {
                model.UpdatedOn = DateTime.UtcNow.ToUniversalTime();

                _dbContext.Update(model);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Houve um erro ao atualizar o metodo de autenticação de 2 fatores");
                throw new BusinessException("Houve um erro ao atualizar o metodo de autenticação de 2 fatores");
            }
        }
    }
}
