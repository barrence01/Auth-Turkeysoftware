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
        /// Realiza a autenticação do usuário com validação de credenciais e código 2FA quando necessário.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/auth/login<br/>
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "password": "SenhaSegura123",
        ///         "twoFactorCode": "123456" (Obrigatório apenas se 2FA estiver habilitado)
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Dados de login (email, senha e código 2FA quando aplicável).</param>
        /// <returns>Retorna tokens de autenticação em cookies HTTP-only e mensagem de status.</returns>
        /// <response code="200">Login realizado com sucesso (tokens armazenados em cookies).</response>
        /// <response code="400">Falha na autenticação (credenciais inválidas, conta bloqueada, 2FA requerido/inválido ou conta não confirmada).</response>
        [HttpPost("login")]
        [TypeFilter(typeof(LoginFilter))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<LoginResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                LoginResponse result = new LoginResponse();

                var user = await _userManager.FindByNameAsync(request.Email);
                if (user == null) {
                    return BadRequest("Email ou senha inválido!", result);
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

                TwoFactorValidationDTO twoFactorResult = await _authenticationService.VerifyTwoFactorAuthentication(user, request.TwoFactorCode);
                if (!twoFactorResult.HasSucceeded())
                {
                    result.IsTwoFactorRequired = true;

                    if (twoFactorResult.IsTwoFactorCodeEmpty)
                    {
                        return BadRequest("É necessário código de autenticação de 2 fatores para o login.", result);
                    }
                    else if (twoFactorResult.IsMaxNumberOfTriesExceeded || twoFactorResult.IsTwoFactorCodeExpired)
                    {
                        result.IsTwoFactorCodeExpired = true;
                        return BadRequest("O código de dois fatores expirou.", result);
                    }
                    else if (twoFactorResult.IsTwoFactorCodeInvalid)
                    {
                        result.IsTwoFactorCodeInvalid = true;
                        return BadRequest("O código 2FA fornecido é inválido", result);
                    }
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var userClaims = await _userManager.GetClaimsAsync(user);

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

                return Ok("Login realizado com sucesso.");
            }
            catch (Exception e) {
                Log.Error(e, "Erro Desconhecido: ");
                return BadRequest("Não foi possível completar o login. Por favor, tente novamente mais tarde.");
            }
        }

        /// <summary>
        /// Envia um código de autenticação de dois fatores (2FA) para o usuário após validação inicial de credenciais.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/auth/send-2fa<br/>
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "password": "SenhaSegura123"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Dados de login (email e senha).</param>
        /// <returns> Retorna uma mensagem de sucesso se o 2FA for enviado ou avisos sobre estado da conta. </returns>
        /// <response code="200"> Código 2FA enviado com sucesso OU usuário não possui 2FA habilitado. </response>
        /// <response code="400"> Credenciais inválidas, conta não confirmada ou bloqueada. </response>
        [HttpPost("send-2fa")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<LoginResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendTwoFactorCode([FromBody] LoginRequest request)
        {
            LoginResponse result = new LoginResponse();

            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null) {
                return BadRequest("Email ou senha inválido!", result);
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

            if (!user.TwoFactorEnabled)
            {
                return Ok("Usuário não possui autenticação de 2 fatores", result);
            }

            await _authenticationService.SendTwoFactorCodeAsync(request.Email);
            return Ok("Código 2FA enviado.");
        }

        /// <summary>
        /// Gera um novo par de tokens de acesso e refresh com base no refresh token fornecido.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/auth/refresh-token<br/>
        ///     {
        ///         // (O refresh token deve ser enviado como cookie)
        ///     }
        /// 
        /// </remarks>
        /// <returns>Retorna 200 (OK) com os novos tokens nos cookies.</returns>
        /// <response code="200">Tokens atualizados com sucesso (armazenados em cookies HTTP-only).</response>
        /// <response code="401">Token inválido ou sessão não autorizada.</response>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<Object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                Request.Cookies.TryGetValue(REFRESH_TOKEN, out string? refreshToken);
                if (string.IsNullOrEmpty(refreshToken))
                    return Unauthorized(ERROR_SESSAO_INVALIDA);

                var principalRefresh = await GetPrincipalFromRefreshToken(refreshToken);

                if (principalRefresh.Identity == null || principalRefresh.Identity.Name == null) {
                    _logger.LogError("Não foi possível identificar a identidade do refresh token.");
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                var user = await _userManager.FindByNameAsync(principalRefresh.Identity.Name);
                if (user == null) {
                    _logger.LogError("Usuário do refresh token não foi encontrado");
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
            catch (BusinessException e)
            {
                _logger.LogError($"RefreshToken não gerado: {e.Message}");
                return Unauthorized(ERROR_SESSAO_INVALIDA);
            }
        }
    }
}
