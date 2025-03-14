using Auth_Turkeysoftware.Models.DTOs;

namespace Auth_Turkeysoftware.Services.MailService
{
    /// <summary>
    /// Interface for email service to send emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="emailRequest">The email request model containing email details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        Task<bool> SendEmailAsync(SendEmailDTO emailRequest);
    }
}
