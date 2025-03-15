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
    [Route("api/auth/[controller]")]
    [ApiController]
    public class LoginController : AuthControllerBase
    {
        private const string ERROR_SESSAO_INVALIDA = "Não foi possível autorizar o token recebido.";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserSessionService _loggedUserService;

        private readonly ILogger<LoginController> _logger;

        public LoginController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtSettingsSingleton jwtSettingsSingleton,
            IUserSessionService loggedUserService,
            ILogger<LoginController> logger) : base(jwtSettingsSingleton)
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
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
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

                var accessToken = GenerateAccessToken(userClaims);
                var refreshToken = GenerateRefreshToken(userClaims);

                UserSessionModel userModel;
                userModel = await _loggedUserService.GetGeolocationByIpAddress(new UserSessionModel
                {
                    IdSessao = newIdSessao,
                    FkIdUsuario = user.Id,
                    RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken),
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

                var principalRefresh = GetPrincipalFromRefreshToken(refreshToken);

                var user = await _userManager.FindByNameAsync(principalRefresh.Identity.Name);
                if (user == null) {
                    _logger.LogWarning($"RefreshToken não gerado: Não foi possível encontrar o Username informado. UserName: {principalRefresh.Identity.Name}");
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                string? idSessao = principalRefresh.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (idSessao == null) {
                    _logger.LogWarning($"RefreshToken não gerado: Não foi possível identificar o claim de sessão. UserName: {principalRefresh.Identity.Name}");
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                if (await _loggedUserService.IsTokenBlackListed(user.Id, idSessao, refreshToken)) {
                    _logger.LogWarning($"RefreshToken não gerado: O refresh token já foi utilizado anteriormente.  UserName: {principalRefresh.Identity.Name}");
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                var newRefreshToken = GenerateRefreshToken(principalRefresh.Claims.ToList());
                var newAccessToken = GenerateAccessToken(principalRefresh.Claims.ToList());

                await _loggedUserService.UpdateSessionRefreshToken(user.Id, idSessao, refreshToken,
                                                                   new JwtSecurityTokenHandler().WriteToken(newRefreshToken));

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
