using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Services.MailService;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Models;
using Serilog;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class AccountRecoveryController : AuthControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public AccountRecoveryController(UserManager<ApplicationUser> userManager, IEmailService emailService,
                                         IConfiguration configuration) : base(configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        /// <summary>
        /// Envia um e-mail de recuperação de senha para o usuário.
        /// TODO: Colocar o domínio correto da página de recuperação.
        /// </summary>
        /// <param name="request">O modelo de solicitação de recuperação de senha.</param>
        /// <returns>Retorna um status de sucesso ou erro.</returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null)
                return BadRequest("Endereço de e-mail inválido.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"https://yourfrontend.com/reset-password?token={Uri.EscapeDataString(token)}&email={request.Email}";

            var emailRequest = new EmailRequestModel
            {
                Body = $"Clique no link para resetar sua senha: {resetLink}",
                Subject = "Recuperação de senha - TurkeySoftware",
                To = new List<string>()
            };

            emailRequest.To.Add(request.Email);

            await _emailService.SendEmailAsync(emailRequest);

            return Ok("E-mail de recuperação de senha enviado.");
        }

        /// <summary>
        /// Reseta a senha do usuário utilizando o código de redefinição.
        /// </summary>
        /// <param name="request">O modelo de solicitação de redefinição de senha.</param>
        /// <returns>Retorna um status de sucesso ou erro.</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null)
                return BadRequest("Endereço de e-mail inválido.");

            var result = await _userManager.ResetPasswordAsync(user, request.ResetCode, request.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return Ok("Senha resetada com sucesso.");
        }
    }
}
