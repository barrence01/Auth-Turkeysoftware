using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : AuthControllerBase
    {
        private const string ERROR_SESSAO_INVALIDA = "Não foi possível autorizar o token recebido.";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserSessionService _loggedUserService;

        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IUserSessionService loggedUserService,
            ILogger<AuthController> logger) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _loggedUserService = loggedUserService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza o login do usuário com base no modelo fornecido.
        /// </summary>
        /// <param name="model">Modelo contendo email e senha do usuário.</param>
        /// <returns>Retorna 200 se OK.</returns>
        [HttpPost]
        [Route("login")]
        [TypeFilter(typeof(LoginFilter))]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                    return BadRequest("Email ou senha inválido!");

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

                if (result.IsLockedOut) {
                    return BadRequest("Sua conta foi bloqueada por excesso de tentativas de login.");
                }
                if (result.IsNotAllowed) {
                    if (!user.EmailConfirmed || !user.PhoneNumberConfirmed || user.TwoFactorEnabled) {
                        return BadRequest("É necessário confirmar a conta antes de fazer login.");
                    }
                }
                if (!result.Succeeded) {
                    return BadRequest("Email ou senha inválido!");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userClaims = await _userManager.GetClaimsAsync(user);
                string newIdSessao = Guid.CreateVersion7().ToString("N");

                // Serve para não criar tokens com a mesma númeração e identificar a sessão
                userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, newIdSessao));

                string accessToken = GenerateAccessToken(userClaims);
                string refreshToken = GenerateRefreshToken(userClaims);

                UserSessionModel userModel;
                userModel = await _loggedUserService.GetGeolocationByIpAddress(new UserSessionModel
                {
                    IdSessao = newIdSessao,
                    FkIdUsuario = user.Id,
                    RefreshToken = refreshToken,
                    IP = HttpContext.Items["IP"]?.ToString() ?? string.Empty,
                    Platform = HttpContext.Items["Platform"]?.ToString() ?? string.Empty,
                    UserAgent = HttpContext.Items["UserAgent"]?.ToString() ?? string.Empty
                });

                await _loggedUserService.AddLoggedUser(userModel);

                AddTokensToCookies(refreshToken, accessToken);

                return Ok();
            }
            catch (Exception e) {
                Log.Error(e, "Erro Desconhecido.");
                return BadRequest("Não foi possível completar o login. Por favor, tente novamente mais tarde.");
            }
        }

        /// <summary>
        /// Gera um novo par de tokens de acesso e refresh token com base no refresh token fornecido.
        /// </summary>
        /// <returns>Retorna 200 se OK.</returns>
        [HttpPost]
        [Route("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                Request.Cookies.TryGetValue(REFRESH_TOKEN, out string? refreshToken);
                if (string.IsNullOrEmpty(refreshToken))
                    return Unauthorized("Refresh Token não encontrado.");

                var principalRefresh = await GetPrincipalFromRefreshToken(refreshToken);

                var user = await _userManager.FindByNameAsync(principalRefresh.Identity.Name);
                if (user == null) {
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                string? idSessao = principalRefresh.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (idSessao == null) {
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                if (await _loggedUserService.IsTokenBlackListed(user.Id, idSessao, refreshToken)) {
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                string newRefreshToken = GenerateRefreshToken(principalRefresh.Claims.ToList());
                string newAccessToken = GenerateAccessToken(principalRefresh.Claims.ToList());

                await _loggedUserService.UpdateSessionRefreshToken(user.Id, idSessao, refreshToken, newRefreshToken);

                AddTokensToCookies(newRefreshToken, newAccessToken);
                return Ok();
            }
            catch (BusinessRuleException e)
            {
                _logger.LogError($"RefreshToken não gerado: {e.Message}");
                return Unauthorized(ERROR_SESSAO_INVALIDA);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Erro desconhecido.");
                return Unauthorized(ERROR_SESSAO_INVALIDA);
            }
        }
    }
}
