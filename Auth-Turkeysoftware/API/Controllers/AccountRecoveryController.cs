using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.API.Bases;
using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Domain.Services.Interfaces;

namespace Auth_Turkeysoftware.API.Controllers
{
    /// <summary>
    /// Controller para recuperação de senha da conta do usuário.<br/>
    /// O acesso à este controller é bloqueado para contas "Guest"
    /// por serem gerenciados pela aplicação que criou a conta.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountRecoveryController : CommonControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IAccountRecoveryService _accountRecoveryService;

        public AccountRecoveryController(UserManager<ApplicationUser> userManager, IAccountRecoveryService accountRecoveryService)
        {
            _userManager = userManager;
            _accountRecoveryService = accountRecoveryService;
        }

        /// <summary>
        /// Envia um e-mail de recuperação de senha para o usuário. TODO: Colocar o domínio correto da página de recuperação.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/AccountRecovery/forgot-password
        ///     {
        ///         "email": "usuario@exemplo.com"
        ///     }
        ///     
        /// O e-mail conterá um link para redefinição de senha com o domínio da aplicação.
        /// </remarks>
        /// <param name="request">Dados do usuário para recuperação de senha.</param>
        /// <returns>Confirmação do envio do e-mail.</returns>
        /// <response code="200">E-mail de recuperação enviado com sucesso.</response>
        /// <response code="400">Endereço de e-mail inválido ou não encontrado.</response>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            string userEmail = request.Email.ToLower();
            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null)
            {
                return BadRequest("Endereço de e-mail inválido.");
            }

            await _accountRecoveryService.SendPasswordResetEmail(user);

            return Ok("E-mail de recuperação de senha enviado.");
        }

        /// <summary>
        /// Redefine a senha do usuário utilizando um token de redefinição válido.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/AccountRecovery/reset-password
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "resetCode": "token-gerado",
        ///         "newPassword": "NovaSenha@123"
        ///     }
        ///     
        /// O token se encontra como paramêtro na URL recebida por email, depois de solicitar /api/AccountRecovery/forgot-password
        /// </remarks>
        /// <param name="request">Dados para redefinição de senha.</param>
        /// <returns>Confirmação da alteração de senha.</returns>
        /// <response code="200">Senha redefinida com sucesso.</response>
        /// <response code="400">Token inválido, e-mail não encontrado ou senha não atende aos requisitos.</response>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<List<string>>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            string userEmail = request.Email.ToLower();
            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null)
            {
                return BadRequest("Endereço de e-mail inválido.");
            }

            var result = await _accountRecoveryService.ResetPassword(user, request);

            if (!result.IsSuccess())
            {
                if (result.Errors.Count > 0)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("Errors", error.Description);
                    }
                }
                ModelState.AddModelError("Errors", "Não foi possível resetar a senha.");
            }

            return Ok("Senha resetada com sucesso.");
        }
    }
}
