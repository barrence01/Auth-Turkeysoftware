using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Repositories
{
    public interface ITwoFactorRepository
    {
        Task AddTwoFactorAuth(TwoFactorAuthModel model);
        Task<TwoFactorAuthModel?> FindByUserIdAndModeAsync(string userId, int twoFactorMode);
        Task<List<TwoFactorAuthModel>> ListAll2FAOptionsAsync(string userId);
        Task<List<TwoFactorAuthModel>> ListActive2FAOptionsAsync(string userId);
        Task UpdateTwoFactorAuth(TwoFactorAuthModel model);
    }
}
