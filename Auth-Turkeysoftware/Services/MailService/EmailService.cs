using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Models.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Auth_Turkeysoftware.Services.MailService
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(SendEmailDTO emailRequest)
        {
            try
            {
                var senderName = _emailSettings.SenderName;
                var senderEmail = _emailSettings.SenderEmail;
                var smtpServer = _emailSettings.SmtpServer;
                var smtpUser = _emailSettings.SmtpUser;
                var smtpSenha = _emailSettings.SmtpPass;
                int smtpPorta = _emailSettings.SmtpPort;

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail));
                email.To.AddRange(emailRequest.To.Select(email => MailboxAddress.Parse(email)));
                email.Subject = emailRequest.Subject;
                email.Body = new TextPart(TextFormat.Html)
                                         { Text = emailRequest.Body };


                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(smtpServer, smtpPorta, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(smtpUser, smtpSenha);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Não foi possível enviar o email solicitado.");
                return false;
            }
        }
    }
}
