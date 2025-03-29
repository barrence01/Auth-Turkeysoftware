using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Authorization;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Controllers
{
    /// <summary>
    /// Controller para recuperação de senha da conta do usuário.<br/>
    /// O acesso à este controller é bloqueado para contas "Guest"
    /// por serem gerenciados pela aplicação que criou a conta.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "DenyGuests")]
    [AllowAnonymous]
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
            string userEmail = request.Email.ToLower();
            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null) {
                return BadRequest("Endereço de e-mail inválido.");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _sendEmailService.SendPasswordResetEmail(resetToken, userEmail);
         
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
            string userEmail = request.Email.ToLower();
            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null) {
                return BadRequest("Endereço de e-mail inválido.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.ResetCode, request.NewPassword);
            if (!result.Succeeded) {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok("Senha resetada com sucesso.");
        }
    }
}
