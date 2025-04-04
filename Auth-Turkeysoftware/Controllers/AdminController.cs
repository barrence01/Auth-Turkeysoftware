using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Bases;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "AcessoElevado")]
    [TypeFilter(typeof(AdminActionLoggingFilterAsync))]
    public class AdminController : AuthControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdministrationService _admnistrationService;

        private const string USER_NOT_FOUND = "Não foi possível encontrar o usuário pelo email fornecido.";

        public AdminController(
            UserManager<ApplicationUser> userManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IAdministrationService admnistrationActionService) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _admnistrationService = admnistrationActionService;
        }

        /// <summary>
        /// Lista todas as sessões ativas de um usuário específico (paginação).
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Admin/ListUserActiveSessions<br/>
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "pagina": 1
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Dados para consulta (email e página).</param>
        /// <returns>Lista paginada de sessões ativas.</returns>
        /// <response code="200">Retorna a lista de sessões ativas.</response>
        /// <response code="400">Usuário não encontrado ou parâmetros inválidos.</response>
        [HttpPost("ListUserActiveSessions")]
        [ProducesResponseType(typeof(Response<PaginationDTO<UserSessionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ListUserActiveSessions([FromBody] ListAllUserSessionsRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);

                if (user == null) {
                    return BadRequest(USER_NOT_FOUND);
                }

                var userActiveSessions = await _admnistrationService.ListUserActiveSessions(user.Id, request.pagina);

                return Ok(userActiveSessions);
            }
            catch (BusinessException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Altera a senha de um usuário (requer permissão Master).
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Admin/ChangeUserPassword<br/>
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "newPassword": "NovaSenha@123",
        ///         "confirmPassword": "NovaSenha@123"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Dados para alteração de senha.</param>
        /// <returns>Resultado da operação.</returns>
        /// <response code="200">Senha alterada com sucesso.</response>
        /// <response code="400">Usuário não encontrado ou falha na operação.</response>
        /// <response code="401">Não autorizado.</response>
        /// <response code="403">Acesso negado (requer role Master).</response>
        [HttpPost("ChangeUserPassword")]
        [Authorize(Roles = nameof(UserRolesEnum.Master))]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ForceChangePasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);

                if (user == null) {
                    return BadRequest(USER_NOT_FOUND);
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
                if (!result.Succeeded)
                    return BadRequest(result.Errors.Select(e => e.Description));

                return Ok();
            }
            catch (BusinessException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Bloqueia uma conta de usuário.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Admin/LockUserAccount<br/>
        ///     {
        ///         "email": "usuario@exemplo.com"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Email do usuário a ser bloqueado.</param>
        /// <returns>Status da conta após bloqueio.</returns>
        /// <response code="200">Conta bloqueada com sucesso.</response>
        /// <response code="400">Usuário não encontrado.</response>
        /// <response code="401">Não autorizado.</response>
        [HttpPost("LockUserAccount")]
        [ProducesResponseType(typeof(Response<UserAccountStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LockUserAccount([FromBody] EmailRequest request) {

            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null || user.UserName == null) {
                return BadRequest(USER_NOT_FOUND);
            }

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddYears(99).ToUniversalTime());

            return Ok("A conta foi bloqueada com sucesso.", new UserAccountStatusResponse(user.UserName, user.LockoutEnabled && user.LockoutEnd > DateTime.Now));
        }

        /// <summary>
        /// Desbloqueia uma conta de usuário.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Admin/UnlockUserAccount<br/>
        ///     {
        ///         "email": "usuario@exemplo.com"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Email do usuário a ser desbloqueado.</param>
        /// <returns>Status da conta após desbloqueio.</returns>
        /// <response code="200">Conta desbloqueada com sucesso.</response>
        /// <response code="400">Usuário não encontrado.</response>
        /// <response code="401">Não autorizado.</response>
        [HttpPost("UnlockUserAccount")]
        [ProducesResponseType(typeof(Response<UserAccountStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UnlockUserAccount([FromBody] EmailRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null || user.UserName == null) {
                return BadRequest(USER_NOT_FOUND);
            }

            await _userManager.SetLockoutEnabledAsync(user, false);
            await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddDays(-1).ToUniversalTime());
            await _userManager.ResetAccessFailedCountAsync(user);

            return Ok("A conta foi desbloqueada com sucesso.", new UserAccountStatusResponse(user.UserName, user.LockoutEnabled && user.LockoutEnd > DateTime.Now));
        }

        /// <summary>
        /// Revoga uma sessão específica de um usuário.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Admin/RevokeUserSession<br/>
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "sessionId": "abc123"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Dados para revogação (email e ID da sessão).</param>
        /// <returns>Resultado da operação.</returns>
        /// <response code="200">Sessão revogada com sucesso.</response>
        /// <response code="400">Usuário não encontrado ou sessão inválida.</response>
        /// <response code="401">Não autorizado.</response>
        [HttpPost("RevokeUserSession")]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeUserSession([FromBody] RevokeUserSessionRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);
                if (user == null) {
                    return BadRequest(USER_NOT_FOUND);
                }

                await _admnistrationService.InvalidateUserSession(user.Id, request.SessionId);

                return Ok("Sessão revogada com sucesso.");
            }
            catch (BusinessException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Revoga todas as sessões ativas de um usuário.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Admin/RevokeAllUserSession<br/>
        ///     {
        ///         "email": "usuario@exemplo.com"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Email do usuário.</param>
        /// <returns>Resultado da operação.</returns>
        /// <response code="200">Todas as sessões foram revogadas.</response>
        /// <response code="400">Usuário não encontrado.</response>
        /// <response code="401">Não autorizado.</response>
        [HttpPost("RevokeAllUserSession")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeAllUserSession([FromBody] EmailRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);
                if (user == null) {
                    return BadRequest(USER_NOT_FOUND);
                }

                await _admnistrationService.InvalidateAllUserSession(user.Id);

                return Ok();
            }
            catch (BusinessException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Obtém o status de bloqueio de uma conta de usuário.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:
        /// 
        ///     POST /api/Admin/GetUserAccountStatus<br/>
        ///     {
        ///         "email": "usuario@exemplo.com"<br/>
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Email do usuário.</param>
        /// <returns>Status da conta e informação de bloqueio.</returns>
        /// <response code="200">Retorna o status da conta.</response>
        /// <response code="400">Usuário não encontrado.</response>
        /// <response code="401">Não autorizado.</response>
        [HttpPost("GetUserAccountStatus")]
        [ProducesResponseType(typeof(Response<UserAccountStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserAccountStatus([FromBody] EmailRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null || user.UserName == null) {
                return BadRequest(USER_NOT_FOUND);
            }

            string returnMessage;
            bool userLockoutStatus = user.LockoutEnabled && user.LockoutEnd > DateTime.Now;
            if (userLockoutStatus) {
                returnMessage = "A conta do usuário está bloqueada.";
            } else {
                returnMessage = "A conta do usuário não está bloqueada.";
            }

            return Ok(returnMessage, new UserAccountStatusResponse(user.UserName, userLockoutStatus));
        }

        /// <summary>
        /// Obtém informações cadastrais de um usuário.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Admin/GetUserInformation<br/>
        ///     {
        ///         "email": "usuario@exemplo.com"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Email do usuário.</param>
        /// <returns>Dados cadastrais do usuário.</returns>
        /// <response code="200">Retorna as informações do usuário.</response>
        /// <response code="400">Usuário não encontrado.</response>
        /// <response code="401">Não autorizado.</response>
        [HttpPost("GetUserInformation")]
        [ProducesResponseType(typeof(Response<UserInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserInformation([FromBody] EmailRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null) {
                return BadRequest(USER_NOT_FOUND);
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
    }
}
