using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
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

        [HttpPost]
        [Route("RevokeUserSessions")]
        public async Task<IActionResult> RevokeUserSessions([FromBody] RevokeUserSessionRequest request) {
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
        [Route("GetUserActiveSessions")]
        public async Task<IActionResult> GetUserActiveSessionsByEmail([FromBody] GetAllUserSessionsRequest request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Email);

                if (user == null) {
                    return BadRequest(USER_NOT_FOUND);
                }

                var userActiveSessions = await _admnistrationService.GetUserActiveSessions(user.Id, request.pagina);

                return Ok(userActiveSessions);
            }
            catch (BusinessRuleException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("ChangeUserPassword")]
        [Authorize(Roles = nameof(UserRolesEnum.Master))]
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
            catch (BusinessRuleException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("DisableOrEnableUserAccount")]
        public async Task<IActionResult> DisableOrEnableUserAccount([FromBody] DisableEnableUserAccountRequest request) {

            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null) {
                return BadRequest(USER_NOT_FOUND);
            }

            string returnMessage = "Nenhuma ação foi tomada.";

            switch (request.operationMode) {
                case 0:
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddYears(99).ToUniversalTime());
                    returnMessage = "A conta foi bloqueada com sucesso.";
                    break;
                case 1:
                    await _userManager.SetLockoutEnabledAsync(user, false);
                    await _userManager.SetLockoutEndDateAsync(user, DateTime.Today.AddDays(-1).ToUniversalTime());
                    await _userManager.ResetAccessFailedCountAsync(user);
                    returnMessage = "A conta foi desbloqueada com sucesso.";
                    break;
                default:
                    return BadRequest("Modo de operação desconhecido.");
            }

            return Ok(returnMessage, new UserAccountStatusResponse(user.UserName, user.LockoutEnabled && user.LockoutEnd > DateTime.Now));
        }

        [HttpPost]
        [Route("GetUserAccountStatus")]
        public async Task<IActionResult> GetUserAccountStatus([FromBody] EmailRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null) {
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

        [HttpPost]
        [Route("GetUserInformation")]
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
