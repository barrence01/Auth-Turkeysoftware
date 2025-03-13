using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Models.DataBaseModels;
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
        private readonly ILoggedUserService _loggedUserService;

        public LoginController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILoggedUserService loggedUserService) : base(configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _loggedUserService = loggedUserService;
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
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                    return Unauthorized("Email ou senha inválido!");

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

                if (result.IsLockedOut) {
                    return Unauthorized("Sua conta foi bloqueada por excesso de tentativas de login.");
                }
                if (result.IsNotAllowed) {
                    if (!user.EmailConfirmed || !user.PhoneNumberConfirmed || user.TwoFactorEnabled) {
                        return Unauthorized("É necessário confirmar a conta antes de fazer login.");
                    }
                }
                if (!result.Succeeded) {
                    return Unauthorized("Email ou senha inválido!");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userClaims = await _userManager.GetClaimsAsync(user);
                string newIdSessao = Guid.CreateVersion7().ToString("N");

                // Serve para não criar tokens com a mesma númeração e identificar a sessão
                userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, newIdSessao));

                var accessToken = GenerateAccessToken(userClaims);
                var refreshToken = GenerateRefreshToken(userClaims);

                LoggedUserModel userModel;
                userModel = await _loggedUserService.GetGeolocationByIpAddress(new LoggedUserModel
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
                    Log.Warning($"RefreshToken não gerado: Não foi possível encontrar o Username informado. UserName: {principalRefresh.Identity.Name}");
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                string? idSessao = principalRefresh.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (idSessao == null) {
                    Log.Warning($"RefreshToken não gerado: Não foi possível identificar o claim de sessão. UserName: {principalRefresh.Identity.Name}");
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                if (await _loggedUserService.IsTokenBlackListed(user.Id, idSessao, refreshToken)) {
                    Log.Warning($"RefreshToken não gerado: O refresh token já foi utilizado anteriormente.  UserName: {principalRefresh.Identity.Name}");
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
                Log.Error($"RefreshToken não gerado: {e.Message}");
                return Unauthorized(ERROR_SESSAO_INVALIDA);
            }
            catch (Exception e)
            {
                Log.Error(e, "Erro desconhecido.");
                return Unauthorized(ERROR_SESSAO_INVALIDA);
            }
        }

        private ClaimsPrincipal GetPrincipalFromRefreshToken(string? refreshToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = true,
                ValidIssuer = getTokenSettings("Issuer"),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(getRefreshSecretKey())),
                ValidateLifetime = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();


            var principal = tokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                      !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                                     StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token inválido.");
            return principal;
        }

        private void AddTokensToCookies(JwtSecurityToken refreshToken, JwtSecurityToken accessToken)
        {
            DeletePreviousTokenFromCookies();

            HttpContext.Response.Cookies.Append(REFRESH_TOKEN, new JwtSecurityTokenHandler().WriteToken(refreshToken),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    Domain = "localhost",
                    Path = "/api/auth/Login/refresh-token",
                    MaxAge = refreshToken.ValidTo.TimeOfDay
                });

            HttpContext.Response.Cookies.Append(ACCESS_TOKEN, new JwtSecurityTokenHandler().WriteToken(accessToken),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict,
                    Domain = "localhost",
                    MaxAge = accessToken.ValidTo.TimeOfDay
                });
        }
    }
}
