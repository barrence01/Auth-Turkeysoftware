using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Bases;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DTOs;
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
    public class UserSessionController : AuthControllerBase
    {
        private const string ERROR_USUARIO_INVALIDO = "Usuário inválido.";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger<UserSessionController> _logger;

        public UserSessionController(
            UserManager<ApplicationUser> userManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IUserSessionService userSessionService,
            ILogger<UserSessionController> logger) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _userSessionService = userSessionService;
            _logger = logger;
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
    }
}
