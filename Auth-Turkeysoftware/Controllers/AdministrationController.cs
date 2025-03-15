using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Models.DTOs;
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
    public class AdministrationController : AuthControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAdministrationService _admnistrationService;

        private const string USER_NOT_FOUND = "Não foi possível encontrar o usuário pelo email fornecido.";

        public AdministrationController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IAdministrationService admnistrationActionService) : base(configuration)
        {
            _userManager = userManager;
            _admnistrationService = admnistrationActionService;
        }

        [HttpPost]
        [Route("revoke-user-sessions")]
        public async Task<IActionResult> RevokeUserSessions([FromBody] RevokeUserSessionDTO request) {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);

                if (user == null) {
                    return BadRequest(USER_NOT_FOUND);
                }

                await _admnistrationService.InvalidateUserSession(user.Id, request.UserSessionId);

                return Ok();
            }
            catch (BusinessRuleException e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost]
        [Route("get-user-all-sessions")]
        public async Task<IActionResult> GetUserAllSessionsByEmail([FromBody] GetAllUserSessionsRequestDTO request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);

                if (user == null) {
                    return BadRequest(USER_NOT_FOUND);
                }

                var userActiveSessions = await _admnistrationService.GetUserAllSessions(user.Id, request.pagina);

                return Ok(userActiveSessions);
            }
            catch (BusinessRuleException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("force-change-user-password")]
        [Authorize(Roles = nameof(UserRolesEnum.Master))]
        public async Task<IActionResult> ForceChangeUserPassword([FromBody] ForceChangePasswordRequestDTO request)
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
            catch (BusinessRuleException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("disable-enable-userAccount")]
        public async Task<IActionResult> DisableOrEnableUserAccount([FromBody] DisableEnableUserAccountRequestDTO request) {

            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null) {
                return BadRequest(USER_NOT_FOUND);
            }

            string returnMessage = "Nenhuma ação foi tomada.";
            if (request.operationMode == 0) {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddYears(99));
                returnMessage = "A conta foi bloqueada com sucesso.";
            }
            else if (request.operationMode == 1) {
                await _userManager.SetLockoutEnabledAsync(user, false);
                await _userManager.ResetAccessFailedCountAsync(user);
                returnMessage = "A conta foi desbloqueada com sucesso.";
            }
            return Ok(returnMessage);
        }

        [HttpPost]
        [Route("get-userAccount-status")]
        public async Task<IActionResult> GetUserAccountStatus([FromBody] EmailRequestDTO request)
        {
            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null) {
                return BadRequest(USER_NOT_FOUND);
            }

            string returnMessage;
            bool userLockoutStatus = user.LockoutEnabled;
            if (userLockoutStatus) {
                returnMessage = "A conta do usuário está bloqueada.";
            } else {
                returnMessage = "A conta do usuário não está bloqueada.";
            }

            return Ok(returnMessage, new { AccountLockedStatus = userLockoutStatus });
        }
    }
}
