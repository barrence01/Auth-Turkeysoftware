using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;

namespace Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Repositories.Interfaces
{
    public interface ITwoFactorRepository
    {
        Task AddTwoFactorAuth(TwoFactorAuthModel model);
        Task<TwoFactorAuthModel?> FindByUserIdAndModeAsync(Guid userId, int twoFactorMode);
        Task<List<TwoFactorAuthModel>> ListAll2FAOptionsAsync(Guid userId);
        Task<List<TwoFactorAuthModel>> ListActive2FAOptionsAsync(Guid userId);
        Task UpdateTwoFactorAuth(TwoFactorAuthModel model);
    }
}
