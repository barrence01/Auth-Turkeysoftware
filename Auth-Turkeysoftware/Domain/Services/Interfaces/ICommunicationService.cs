namespace Auth_Turkeysoftware.Domain.Services.Interfaces
{
    public interface ICommunicationService
    {
        Task Send2FAEmailAsync(string email, string twoFactorCode, string tokenLifeSpanInMinutes);
        Task SendEnable2FAEmailAsync(string email, string twoFactorCode, string tokenLifeSpanInMinutes);
        Task SendPasswordResetEmail(string email, string resetToken);
        Task SendConfirmEmailRequest(string userId, string email, string confirmToken);
    }
}
