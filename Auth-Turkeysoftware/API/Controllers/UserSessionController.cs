using Auth_Turkeysoftware.Infraestructure.Configurations.Singletons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth_Turkeysoftware.API.Bases;
using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Shared.Exceptions;
using Auth_Turkeysoftware.Domain.Models.VOs;
using Auth_Turkeysoftware.Domain.Services.Interfaces;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities.Identity;

namespace Auth_Turkeysoftware.API.Controllers
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class UserSessionController : AuthControllerBase
    {
        private const string ERROR_USUARIO_INVALIDO = "Usuário inválido.";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserSessionService _userSessionService;

        public UserSessionController(
            UserManager<ApplicationUser> userManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IUserSessionService userSessionService) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _userSessionService = userSessionService;
        }

        /// <summary>
        /// Obtém todas as sessões ativas do usuário com paginação.
        /// </summary>
        /// <param name="pagina">Número da página (iniciando em 1).</param>
        /// <returns>Lista paginada de sessões ativas do usuário autenticado.</returns>
        /// <response code="200">Retorna a lista de sessões ativas com metadados de paginação.</response>
        /// <response code="400">Usuário inválido ou parâmetros incorretos.</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpGet("all-sessions")]
        [ProducesResponseType(typeof(Response<PaginationVO<UserSessionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ListAllSessions([FromQuery] int pagina)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return BadRequest(ERROR_USUARIO_INVALIDO);
                }

                var userActiveSessions = await _userSessionService.ListUserActiveSessionsPaginated(new Guid(userId), pagina);

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
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
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
            if (user == null)
            {
                return Ok();
            }

            await _userSessionService.InvalidateUserSession(user.Id, new Guid(sessionId));

            return Ok();
        }

        /// <summary>
        /// Revoga uma sessão específica do usuário autenticado.
        /// </summary>
        /// <param name="sessionId">Identificador único da sessão a ser revogada.</param>
        /// <returns>Resultado da operação de revogação.</returns>
        /// <response code="200">Sessão revogada com sucesso.</response>
        /// <response code="400">Falha na operação (usuário inválido, sessão não encontrada ou permissão negada).</response>
        /// <response code="401">Não autorizado - token inválido ou ausente.</response>
        [HttpPost("revoke-session/{sessionId}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeSession([FromRoute] string sessionId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                if (userEmail == null)
                {
                    return BadRequest(ERROR_USUARIO_INVALIDO);
                }

                var user = await _userManager.FindByNameAsync(userEmail);

                if (user == null || userId == null)
                {
                    return BadRequest(ERROR_USUARIO_INVALIDO);
                }

                if (user.Id != new Guid(userId))
                {
                    return BadRequest(ERROR_USUARIO_INVALIDO);
                }

                await _userSessionService.InvalidateUserSession(user.Id, new Guid(sessionId));

                return Ok();
            }
            catch (BusinessException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
