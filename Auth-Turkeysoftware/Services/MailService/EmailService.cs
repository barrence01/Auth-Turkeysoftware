using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Models.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Auth_Turkeysoftware.Services.MailService
{
    public class EmailService : IEmailService
    {
        protected readonly AuthEmailSettingsSingleton _emailSettings;

        private readonly ILogger<EmailService> _logger;

        public EmailService(AuthEmailSettingsSingleton emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(SendEmailDto emailRequest)
        {
            try
            {
                var senderName = _emailSettings.GetEmailSettings().SenderName;
                var senderEmail = _emailSettings.GetEmailSettings().SenderEmail;
                var smtpServer = _emailSettings.GetEmailSettings().SmtpServer;
                var smtpUser = _emailSettings.GetEmailSettings().SmtpUser;
                var smtpSenha = _emailSettings.GetEmailSettings().SmtpPass;
                int smtpPorta = _emailSettings.GetEmailSettings().SmtpPort;

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
