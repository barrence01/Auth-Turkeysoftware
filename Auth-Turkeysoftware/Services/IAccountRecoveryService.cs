namespace Auth_Turkeysoftware.Services
{
    public interface IAccountRecoveryService
    {
        Task SendPasswordResetEmail(string resetToken, string userEmail);
    }
}
