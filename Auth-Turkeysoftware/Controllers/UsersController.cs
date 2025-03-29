using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Base;
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
    public class UsersController : AuthControllerBase
    {
        private const string ERROR_USUARIO_INVALIDO = "Usuário inválido.";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserSessionService _userSessionService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IUserSessionService userSessionService) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _userSessionService = userSessionService;
        }

        [HttpGet]
        [Route("get-info")]
        public async Task<IActionResult> GetUserInfo() { 

            string userName = User.FindFirst(ClaimTypes.Name).Value;
            var user = await _userManager.FindByNameAsync(userName);
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
        /// Obtém todas as sessões ativas do usuário.
        /// </summary>
        /// <param name="pagina">Número da página para paginação.</param>
        /// <returns>Um <see cref="PaginationDTO&lt;List&lt;UserSessionResponse&gt;&gt;" /> contendo as sessões ativas do usuário.</returns>
        [HttpGet]
        [Route("all-sessions")]
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
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            if (model == null) {
                return BadRequest("Invalid request");
            }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
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

        [HttpPost]
        [Route("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated) {
                return Ok();
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            string? idSessao = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            DeletePreviousTokenFromCookies();

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) {
                return Ok(ERROR_USUARIO_INVALIDO);
            }

            await _userSessionService.InvalidateUserSession(idSessao, user.Id);

            return Ok();
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
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
            catch (BusinessRuleException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Deleta a conta do usuário.
        /// </summary>
        /// <returns>Um 200 OK indicando o resultado da operação.</returns>
        [HttpPost]
        [Route("delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
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
    }
}
