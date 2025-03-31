using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Base;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
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
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            JwtSettingsSingleton jwtSettingsSingleton,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUserSessionService loggedUserService,
            IAuthenticationService authenticationService,
            ILogger<AuthController> logger) : base(jwtSettingsSingleton)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _loggedUserService = loggedUserService;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza o login do usuário com base no modelo fornecido.
        /// </summary>
        /// <param name="request">Modelo contendo email e senha do usuário.</param>
        /// <returns>Retorna 200 se OK.</returns>
        [HttpPost]
        [Route("login")]
        [TypeFilter(typeof(LoginFilter))]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                LoginResponse result = new LoginResponse();

                var user = await _userManager.FindByNameAsync(request.Email);
                if (user == null) {
                    return BadRequest("Email ou senha inválido!", result);
                }

                var twoFactorResult = _authenticationService.VerifyTwoFactor(user, request.TwoFactorCode);
                if (!twoFactorResult.HasSucceeded()) { 
                    result.IsTwoFactorRequired = true;
                    result.HasTwoFactorFailed = true;

                    if (twoFactorResult.IsTwoFactorCodeEmpty) {
                        return BadRequest("É necessário código de autenticação de 2 fatores para o login.", result);
                    }
                    else if (twoFactorResult.IsMaxNumberOfTriesExceeded) {
                        result.HasTwoFactorCodeExpired = true;
                        return BadRequest("O número máximo de tentativas foi excedido.", result);
                    }
                    else if (twoFactorResult.IsTwoFactorCodeExpired) {
                        result.HasTwoFactorCodeExpired = true;
                        return BadRequest("O código de 2 fatores está expirado.", result);
                    }
                    else if (twoFactorResult.IsTwoFactorCodeInvalid) {
                        return BadRequest("O código 2FA fornecido é inválido", result);
                    }
                }


                var signInresult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (!signInresult.Succeeded)
                {
                    if (signInresult.IsLockedOut) {
                        result.IsAccountLockedOut = true;
                        return BadRequest("Sua conta foi bloqueada por excesso de tentativas de login.", result);
                    }

                    if (signInresult.IsNotAllowed) {
                        if (!user.EmailConfirmed || !user.PhoneNumberConfirmed) {
                            return BadRequest("É necessário confirmar a conta antes de fazer login.", result);
                        }
                    }
                
                    return BadRequest("Email ou senha inválido!", result);
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userClaims = await _userManager.GetClaimsAsync(user);

                // Serve para não criar tokens com a mesma númeração e identificar a sessão com um id único
                string newIdSessao = Guid.CreateVersion7().ToString("N");
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

                result.HasSucceeded = true;
                return Ok(result);
            }
            catch (Exception e) {
                Log.Error(e, "Erro Desconhecido.");
                return BadRequest("Não foi possível completar o login. Por favor, tente novamente mais tarde.");
            }
        }

        [HttpPost("send-2fa")]
        [AllowAnonymous]
        public async Task<IActionResult> SendTwoFactorCode([FromBody] LoginRequest request)
        {
            LoginResponse result = new LoginResponse();

            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null) {
                return BadRequest("Email ou senha inválido!", result);
            }

            if (!user.TwoFactorEnabled) {
                result.HasSucceeded = true;
                return Ok("Usuário não possui autenticação de 2 fatores", result);
            }

            var signInresult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (!signInresult.Succeeded) {
                if (signInresult.IsLockedOut) {
                    result.IsAccountLockedOut = true;
                    return BadRequest("Sua conta foi bloqueada por excesso de tentativas de login.", result);
                }

                if (signInresult.IsNotAllowed) {
                    if (!user.EmailConfirmed || !user.PhoneNumberConfirmed) {
                        return BadRequest("É necessário confirmar a conta antes de fazer login.", result);
                    }
                }
                return BadRequest("Email ou senha inválido!", result);
            }

            await _authenticationService.SendTwoFactorCodeAsync(request.Email);
            result.HasSucceeded = true;
            return Ok("Código 2FA enviado.", result);
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
                    return Unauthorized(ERROR_SESSAO_INVALIDA);

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
