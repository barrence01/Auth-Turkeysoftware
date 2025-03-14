using Auth_Turkeysoftware.Models.DataBaseModels;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Auth_Turkeysoftware.Controllers.Base;
using Serilog;
using Auth_Turkeysoftware.Services;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/recovery/[controller]")]
    [ApiController]
    public class AccountRecoveryController : CommonControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SendEmailService _sendEmailService;

        public AccountRecoveryController(UserManager<ApplicationUser> userManager, SendEmailService sendEmailService)
        {
            _userManager = userManager;
            _sendEmailService = sendEmailService;
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

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _sendEmailService.SendPasswordResetEmail(resetToken, request.Email);
         
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
