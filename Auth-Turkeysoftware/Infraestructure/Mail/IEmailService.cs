using Auth_Turkeysoftware.Domain.Models.VOs;

namespace Auth_Turkeysoftware.Infraestructure.Mail
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
        Task<bool> SendEmailAsync(SendEmailVO emailRequest);
    }
}
