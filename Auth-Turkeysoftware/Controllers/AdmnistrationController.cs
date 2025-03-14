using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Policy = "AcessoElevado")]
    public class AdmnistrationController : AuthControllerBase
    {
        private const string ERROR_SESSAO_INVALIDA = "Não foi possível autorizar o token recebido.";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAdministrationService _admnistrationService;

        public AdmnistrationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IAdministrationService admnistrationActionService) : base(configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _admnistrationService = admnistrationActionService;
        }

        [HttpPost]
        [Route("revoke-user-all-sessions")]
        public async Task<IActionResult> RevogarSessoesUsuarioPorEmail([FromBody] EmailRequestDTO emailRequest) {
            try
            {
                var user = await _userManager.FindByNameAsync(emailRequest.Email);

                await _admnistrationService.InvalidateAllUserSession(user.Id);

                return Ok();
            }
            catch (BusinessRuleException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
