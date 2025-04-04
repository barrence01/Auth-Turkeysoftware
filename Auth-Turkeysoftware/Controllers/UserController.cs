using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Bases;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : AuthControllerBase
    {
        private const string ERROR_USUARIO_INVALIDO = "Usuário inválido.";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<ApplicationUser> userManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IUserSessionService userSessionService,
            ILogger<UserController> logger) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _userSessionService = userSessionService;
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
        /// Obtém todas as sessões ativas do usuário com paginação.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        ///
        ///     GET /api/User/all-sessions?pagina=1<br/>
        ///     Authorization: Bearer {token}
        ///
        /// </remarks>
        /// <param name="pagina">Número da página (iniciando em 1).</param>
        /// <returns>Lista paginada de sessões ativas do usuário autenticado.</returns>
        /// <response code="200">Retorna a lista de sessões ativas com metadados de paginação.</response>
        /// <response code="400">Usuário inválido ou parâmetros incorretos.</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpGet("all-sessions")]
        [ProducesResponseType(typeof(Response<PaginationDTO<UserSessionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ListAllSessions([FromQuery] int pagina)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) {
                    return BadRequest(ERROR_USUARIO_INVALIDO);
                }

                var userActiveSessions = await _userSessionService.ListUserActiveSessionsPaginated(userId, pagina);

                return Ok(userActiveSessions);
            }
            catch (BusinessException e)
            {
                return BadRequest(e.Message);
            }
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
        /// <param name="model">Dados necessários para alteração de senha (senha atual e nova senha).</param>
        /// <returns>Resultado da operação de alteração de senha.</returns>
        /// <response code="200">Senha alterada com sucesso.</response>
        /// <response code="400">Falha na operação (dados inválidos, usuário não encontrado ou senha atual incorreta).</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            if (model == null) {
                return BadRequest("Invalid request");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            if (userEmail == null) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }

            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null || userId == null) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }

            if (user.Id != userId) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded) {
                return BadRequest("Não foi possível alterar a senha do usuário.", result.Errors);
            }

            return Ok("Senha alterada com sucesso.");
        }

        /// <summary>
        /// Realiza o logout do usuário invalidando a sessão atual.
        /// </summary>
        /// <remarks>
        /// Remove os tokens de autenticação e invalida a sessão no servidor.<br/>
        /// 
        /// Pode ser chamado mesmo sem autenticação ativa (sempre retorna 200).
        /// </remarks>
        /// <returns>Resultado da operação de logout.</returns>
        /// <response code="200">Logout realizado com sucesso (mesmo para sessões inválidas).</response>
        [HttpGet("logout")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated) {
                return Ok();
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            string? sessionId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            DeletePreviousTokenFromCookies();

            if (userName == null || sessionId == null)
            {
                return Ok();
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) {
                return Ok();
            }

            await _userSessionService.InvalidateUserSession(user.Id, sessionId);

            return Ok();
        }

        /// <summary>
        /// Revoga uma sessão específica do usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/User/revoke-session/{sessionId}
        ///     Authorization: Bearer {token}
        ///
        /// </remarks>
        /// <param name="idSessao">Identificador único da sessão a ser revogada.</param>
        /// <returns>Resultado da operação de revogação.</returns>
        /// <response code="200">Sessão revogada com sucesso.</response>
        /// <response code="400">Falha na operação (usuário inválido, sessão não encontrada ou permissão negada).</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpPost("revoke-session/{idSessao}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeSession([FromRoute] string idSessao)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
                if (userEmail == null) {
                    return BadRequest(ERROR_USUARIO_INVALIDO);
                }

                var user = await _userManager.FindByNameAsync(userEmail);

                if (user == null || userId == null) {
                    return BadRequest(ERROR_USUARIO_INVALIDO);
                }

                if (user.Id != userId) {
                    return BadRequest(ERROR_USUARIO_INVALIDO);
                }

                await _userSessionService.InvalidateUserSession(user.Id, idSessao);

                return Ok();
            }
            catch (BusinessException e)
            {
                return BadRequest(e.Message);
            }
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
        /// Observação: Esta operação é irreversível e remove todos os dados do usuário.
        /// </remarks>
        /// <returns>Resultado da operação de exclusão da conta.</returns>
        /// <response code="200">Conta removida com sucesso.</response>
        /// <response code="400">Falha na operação (usuário inválido ou não encontrado).</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpPost("delete-account")]
        [ProducesResponseType(typeof(Response<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            if (userEmail == null) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }

            var user = await _userManager.FindByNameAsync(userEmail);

            if (user == null || userId == null) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }

            if (user.Id != userId) {
                return BadRequest(ERROR_USUARIO_INVALIDO);
            }

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
        public async Task<IActionResult> EnableTwoFactor() {
            return Ok();
        }
    }
}
