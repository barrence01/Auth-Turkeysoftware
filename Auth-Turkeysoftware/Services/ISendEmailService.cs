namespace Auth_Turkeysoftware.Services
{
    public interface ISendEmailService
    {
        Task SendPasswordResetEmail(string resetToken, string userEmail);
    }
}
