using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/Auth/[controller]")]
    [ApiController]
    [Authorize]
    public class UserManagementController : AuthControllerBase
    {
        private const string ERROR_USUARIO_INVALIDO = "Usuário inválido.";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILoggedUserService _loggedUserService;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILoggedUserService loggedUserService) : base(configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _loggedUserService = loggedUserService;
        }

        /// <summary>
        /// Revoga a sessão do usuário especificado.
        /// </summary>
        /// <param name="idSessao">ID da sessão a ser revogada.</param>
        /// <returns>Um 200 OK indicando o resultado da operação.</returns>
        [HttpPost]
        [Route("revoke-session/{idSessao}")]
        public async Task<IActionResult> Revoke([FromRoute] string idSessao)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
                var user = await _userManager.FindByNameAsync(userEmail);
                if (user == null || userId == null)
                    return Unauthorized(ERROR_USUARIO_INVALIDO);

                if (user.Id != userId)
                    return Unauthorized(ERROR_USUARIO_INVALIDO);

                await _loggedUserService.InvalidateUserSession(idSessao, user.Id);

                return Ok();
            }
            catch (BusinessRuleException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtém todas as sessões ativas do usuário.
        /// </summary>
        /// <param name="pagina">Número da página para paginação.</param>
        /// <returns>Um <see cref="PaginationModel&lt;List&lt;UserSessionModel&gt;&gt;" /> contendo as sessões ativas do usuário.</returns>
        [HttpPost]
        [Route("all-sessions")]
        public async Task<IActionResult> GetAllSessions([FromQuery] int pagina)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (userId == null)
                    return Unauthorized(ERROR_USUARIO_INVALIDO);

                var userActiveSessions = await _loggedUserService.GetUserActiveSessions(userId, pagina);

                return Ok(userActiveSessions);
            }
            catch (BusinessRuleException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Altera a senha do usuário.
        /// </summary>
        /// <param name="model">Modelo contendo a senha atual e a nova senha.</param>
        /// <returns>Um 200 OK indicando o resultado da operação.</returns>
        [HttpPost]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (model == null)
                return BadRequest("Invalid request");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null || userId == null)
                return Unauthorized(ERROR_USUARIO_INVALIDO);

            if (user.Id != userId)
                return Unauthorized(ERROR_USUARIO_INVALIDO);

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Senha alterada com sucesso.");
        }

        /// <summary>
        /// Deleta a conta do usuário.
        /// </summary>
        /// <returns>Um 200 OK indicando o resultado da operação.</returns>
        [HttpPost]
        [Route("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null || userId == null)
                return Unauthorized(ERROR_USUARIO_INVALIDO);

            if (user.Id != userId)
                return Unauthorized(ERROR_USUARIO_INVALIDO);

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            DeletePreviousTokenFromCookies();

            return Ok("Conta deletada com sucesso");
        }
    }
}
