using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Bases;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Models.Results;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : AuthControllerBase
    {
        private const string ERROR_USUARIO_INVALIDO = "Usuário inválido.";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger<UserSessionController> _logger;

        public UserController(
            UserManager<ApplicationUser> userManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IUserService userService,
            ILogger<UserSessionController> logger) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém as informações básicas do usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Requer autenticação via token JWT.
        /// </remarks>
        /// <returns>Dados cadastrais do usuário autenticado.</returns>
        /// <response code="200">Retorna as informações do usuário.</response>
        /// <response code="400">Usuário inválido ou não encontrado.</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpGet("get-info")]
        [ProducesResponseType(typeof(Response<UserInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserInfo() { 

            string? userName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userName == null) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) { 
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }
            return Ok(new UserInfoResponse
                     {
                         Name = user.Name,
                         Email = user.UserName,
                         Phone = user.PhoneNumber,
                         EmailConfirmed = user.EmailConfirmed,
                         PhoneConfirmed = user.PhoneNumberConfirmed,
                         TwoFactorEnabled = user.TwoFactorEnabled
                     });
        }

        /// <summary>
        /// Altera a senha do usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/User/change-password<br/>
        ///     {
        ///         "currentPassword": "SenhaAtual@123",
        ///         "newPassword": "NovaSenha@456",
        ///         "confirmPassword": "NovaSenha@456"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Dados necessários para alteração de senha (senha atual e nova senha).</param>
        /// <returns>Resultado da operação de alteração de senha.</returns>
        /// <response code="200">Senha alterada com sucesso.</response>
        /// <response code="400">Falha na operação (dados inválidos, usuário não encontrado ou senha atual incorreta).</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByNameAsync(userName!);
            if (user == null || userName == null) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }
            else if (user.Id != userId) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }
            else if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword)) {
                return BadRequest("Senha inválida");
            };

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded) {
                return BadRequest("Não foi possível alterar a senha do usuário.", result.Errors);
            }

            return Ok("Senha alterada com sucesso.");
        }

        /// <summary>
        /// Remove permanentemente a conta do usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/User/delete-account
        ///     Authorization: Bearer {token}
        ///
        /// Observação: Esta operação é irreversível e remove todos os dados de login do usuário.
        /// </remarks>
        /// <returns>Resultado da operação de exclusão da conta.</returns>
        /// <response code="200">Conta removida com sucesso.</response>
        /// <response code="400">Falha na operação (usuário inválido ou não encontrado).</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpPost("delete-account")]
        [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteAccount([FromBody] PasswordRequest request)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByNameAsync(userEmail!);

            if (userEmail == null) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }
            else if (user == null || userName == null) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }
            else if (user.Id != userId) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }
            else if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return BadRequest("Senha inválida");
            };

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded) {
                return BadRequest(result.Errors);
            }

            DeletePreviousTokenFromCookies();

            return Ok("Conta deletada com sucesso");
        }

        /// <summary>
        /// Habilita a autenticação de dois fatores (2FA) para o usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/User/enable-two-factor
        ///     Authorization: Bearer {token}
        ///
        /// </remarks>
        /// <returns>Resultado da operação.</returns>
        /// <response code="200">2FA habilitado com sucesso.</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpPost("enable-two-factor")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequest request)
        {
            EnableTwoFactorResponse response = new EnableTwoFactorResponse();

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            var user = await _userManager.FindByNameAsync(userName!);
            if (user == null) {
                throw new BusinessException("Usuário não encontrado");
            }

            if (!user.EmailConfirmed) {
                response.IsEmailNotConfirmed = true;
                return BadRequest(response);
            }


            if(!await _userManager.CheckPasswordAsync(user, request.Password)) {
                response.IsPasswordInvalid = true;
                return BadRequest(response);
            }


            switch (request.OperationMode) {
                case (int) TwoFactorModeEnum.EMAIL:
                    await _userService.RequestEnable2FAByEmail(user, userName!);
                    break;
                default:
                    throw new NotImplementedException("Tipo de autenticação de 2 fatores não implementada.");
            }

            
            return Ok();
        }

        /// <summary>
        /// Habilita a autenticação de dois fatores (2FA) para o usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/User/confirm-enable-two-factor
        ///     Authorization: Bearer {token}
        ///
        /// </remarks>
        /// <returns>Resultado da operação.</returns>
        /// <response code="200">2FA habilitado com sucesso.</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpPost("confirm-enable-two-factor")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConfirmEnableTwoFactor([FromBody] ConfirmEnableTwoFactorRequest request)
        {
            EnableTwoFactorResponse response = new EnableTwoFactorResponse();

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            var user = await _userManager.FindByNameAsync(userName!);
            if (user == null) {
                throw new BusinessException("Usuário não encontrado");
            }
            else if (!user.EmailConfirmed) {
                response.IsEmailNotConfirmed = true;
                return BadRequest(response);
            }

            TwoFactorValidationResult? twoFactorResult = null;

            switch (request.OperationMode)
            {
                case (int)TwoFactorModeEnum.EMAIL:
                    twoFactorResult = await _userService.ConfirmEnable2FA(user, request.Code);
                    break;
                default:
                    throw new NotImplementedException("Tipo de autenticação 2FA não implementada.");
            }

            if (!twoFactorResult.HasSucceeded())
            {
                response.IsTwoFactorCodeInvalid = true;
                if (twoFactorResult.IsTwoFactorCodeEmpty) {
                    return BadRequest("É necessário código de autenticação 2FA para a confirmação de 2FA.", response);
                }
                else if (twoFactorResult.IsMaxNumberOfTriesExceeded || twoFactorResult.IsTwoFactorCodeExpired)
                {
                    response.IsTwoFactorCodeExpired = true;
                    return BadRequest("O código de 2FA expirou.", response);
                }
                else if (twoFactorResult.IsTwoFactorCodeInvalid)
                {
                    response.IsTwoFactorCodeInvalid = true;
                    return BadRequest("O código 2FA fornecido é inválido", response);
                }
                throw new BusinessException("Houve um erro desconhecido durante a confirmação de 2FA.");
            }

            return Ok();
        }
    }
}
