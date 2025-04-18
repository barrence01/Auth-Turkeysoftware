using Auth_Turkeysoftware.Configurations.Services;
using Auth_Turkeysoftware.Controllers.Bases;
using Auth_Turkeysoftware.Controllers.Filters;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models.Request;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Models.Result;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : AuthControllerBase
    {
        private const string ERROR_SESSAO_INVALIDA = "Não foi possível reconhecer a sessão do usuário";
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
        /// Verifica as credenciais do usuário e inicia o processo de autenticação.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Auth/try-login<br/>
        ///     {
        ///         "email": "usuario@exemplo.com",
        ///         "password": "SenhaSegura123"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request">Dados de login contendo email e senha.</param>
        /// <returns>
        /// Resultado da tentativa de login. Pode indicar necessidade de 2FA e retorna token temporário em cookies HTTP-only.
        /// </returns>
        /// <response code="200">
        /// Retorna:
        /// - Token temporário em cookies HTTP-only se credenciais válidas
        /// - IsTwoFactorRequired=true se 2FA necessário
        /// </response>
        /// <response code="400">
        /// Falha devido a:
        /// - Credenciais inválidas
        /// - Conta bloqueada
        /// - Conta não confirmada
        /// </response>
        [HttpPost("try-login")]
        [TypeFilter(typeof(LoginFilter))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<TryLoginResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<TryLoginResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TryLogin([FromBody] TryLoginRequest request)
        {
            try
            {
                TryLoginResponse response = new TryLoginResponse();

                var user = await _userManager.FindByNameAsync(request.Email);
                if (user == null) {
                    return BadRequest("Email ou senha inválido!", response);
                }

                var signInresult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (!signInresult.Succeeded) {
                    if (signInresult.IsLockedOut) {
                        response.IsAccountLockedOut = true;
                        return BadRequest("Sua conta foi bloqueada por excesso de tentativas de login.", response);
                    }

                    if (signInresult.IsNotAllowed) {
                        if (!user.EmailConfirmed || !user.PhoneNumberConfirmed) {
                            return BadRequest("É necessário confirmar a conta antes de fazer login.", response);
                        }
                        response.IsPasswordEmailInvalid = true;
                    }
                    response.IsPasswordEmailInvalid = true;
                    return BadRequest("Email ou senha inválido!", response);
                }

                if (user.TwoFactorEnabled) {
                    response.IsTwoFactorRequired = true;
                }

                var userClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName!)
                };

                string loginToken = GenerateLoginToken(userClaims);
                AddLoginTokenToCookies(loginToken);

                response.IsSuccess = true;
                return Ok(response);
            }
            catch (Exception e)
            {
                Log.Error(e, "Erro Desconhecido: ");
                return BadRequest("Não foi possível completar o login. Por favor, tente novamente mais tarde.");
            }
        }

        /// <summary>
        /// Completa o processo de autenticação com validação de código 2FA quando necessário.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Auth/login<br/>
        ///     {
        ///         "twoFactorCode": "123456",
        ///         "twoFactorMode": 1,
        ///     }
        ///     
        /// Requer cookie com token temporário gerado por TryLogin.
        /// </remarks>
        /// <param name="request">Dados contendo código 2FA quando aplicável.</param>
        /// <returns>Tokens de autenticação completos em cookies HTTP-only.</returns>
        /// <response code="200">Login realizado com sucesso.</response>
        /// <response code="400">
        /// Falha devido a:
        /// - Código 2FA inválido/expirado
        /// - Sessão inválida
        /// - Limite de tentativas excedido
        /// </response>
        [HttpPost("login")]
        [TypeFilter(typeof(LoginFilter))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<LoginResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] ValidateLoginRequest request)
        {
            try
            {
                var user = await ValidateLoginToken();
                if (user == null) {
                    return BadRequest(ERROR_SESSAO_INVALIDA);
                }

                LoginResponse response = new LoginResponse();

                TwoFactorValidationResult twoFactorResult = await _authenticationService.VerifyTwoFactorAuthentication(user, request.TwoFactorCode);
                if (!twoFactorResult.HasSucceeded())
                {
                    response.IsTwoFactorRequired = true;

                    if (twoFactorResult.IsTwoFactorCodeEmpty) {
                        return BadRequest("É necessário inserir o código de autenticação de 2 fatores para o login.", response);
                    }
                    else if (twoFactorResult.IsMaxNumberOfTriesExceeded || twoFactorResult.IsTwoFactorCodeExpired)
                    {
                        if (twoFactorResult.IsMaxNumberOfTriesExceeded) {
                            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(30)));
                        }

                        response.IsTwoFactorCodeExpired = true;
                        return BadRequest("O código de 2 fatores expirou.", response);
                    }
                    else if (twoFactorResult.IsTwoFactorCodeInvalid)
                    {
                        response.IsTwoFactorCodeInvalid = true;
                        return BadRequest("O código de 2 fatores fornecido é inválido", response);
                    }
                    throw new BusinessException("Houve um erro desconhecido durante o login.");
                }

                var userClaims = await _userManager.GetClaimsAsync(user);

                string newIdSessao = Guid.CreateVersion7().ToString("N");
                userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, newIdSessao));

                string accessToken = GenerateAccessToken(userClaims);
                string refreshToken = GenerateRefreshToken(userClaims);

                UserSessionModel userModel;
                userModel = new UserSessionModel
                {
                    SessionId = newIdSessao,
                    FkUserId = user.Id,
                    RefreshToken = refreshToken,
                    IP = HttpContext.Items["IP"]?.ToString() ?? string.Empty,
                    Platform = HttpContext.Items["Platform"]?.ToString() ?? string.Empty,
                    UserAgent = HttpContext.Items["UserAgent"]?.ToString() ?? string.Empty
                };

                await _loggedUserService.AddLoggedUser(userModel);

                AddTokensToCookies(refreshToken, accessToken);

                return Ok("Login realizado com sucesso.");
            }
            catch (BusinessException e)
            {
                return BadRequest(e.Message);
            }
            catch (SecurityTokenException)
            {
                return BadRequest(ERROR_SESSAO_INVALIDA);
            }
            catch (Exception e)
            {
                Log.Error(e, "Erro Desconhecido: ");
                return BadRequest("Não foi possível completar o login. Por favor, tente novamente mais tarde.");
            }
        }

        /// <summary>
        /// Envia código de autenticação de dois fatores para o usuário.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Auth/send-2fa<br/>
        ///     {
        ///         "twoFactorMode": 1
        ///     }
        ///     
        /// Requer cookie com token temporário gerado por TryLogin.
        /// </remarks>
        /// <param name="request">Dados contendo modo de autenticação 2FA.</param>
        /// <returns>Confirmação de envio do código 2FA.</returns>
        /// <response code="200">Código 2FA enviado com sucesso.</response>
        /// <response code="400">
        /// Falha devido a:
        /// - Modo 2FA inválido
        /// - Sessão inválida
        /// - 2FA não habilitado
        /// </response>
        [HttpPost("send-2fa")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<LoginResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendTwoFactorCode([FromBody] ValidateLoginRequest request)
        {
            try
            {
                var user = await ValidateLoginToken();
                if (user == null) {
                    return BadRequest(ERROR_SESSAO_INVALIDA);
                }

                if (!user.TwoFactorEnabled) {
                    return Ok("Usuário não possui autenticação de 2 fatores");
                }

                if (request.TwoFactorMode <= 0) {
                    ModelState.AddModelError("twoFactorMode", "TwoFactorMode não pode ser nulo.");
                    return BadRequest("TwoFactorMode não pode ser nulo.");
                }

                await _authenticationService.SendTwoFactorCodeAsync(user, request.TwoFactorMode);
                return Ok("Código de 2 fatores enviado.");
            }
            catch (BusinessException e)
            {
                return BadRequest(e.Message);
            }
            catch (SecurityTokenException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Lista os métodos de autenticação de dois fatores disponíveis para o usuário.
        /// </summary>
        /// <remarks>
        /// Requer cookie com token temporário gerado por TryLogin.
        /// </remarks>
        /// <returns>Lista de métodos 2FA disponíveis.</returns>
        /// <response code="200">Lista de opções 2FA ou indicação que 2FA não está habilitado.</response>
        /// <response code="400">Sessão inválida ou usuário não encontrado.</response>
        [HttpPost("list-2fa-options")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Response<List<TwoFactorAuthResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<LoginResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ListUserTwoFactorOptions()
        {
            try
            {
                var user = await ValidateLoginToken();
                if (user == null) {
                    return BadRequest(ERROR_SESSAO_INVALIDA);
                }

                return Ok(await _authenticationService.ListUserTwoFactorOptions(user));
            }
            catch (BusinessException e) {
                return BadRequest(e.Message);
            }
            catch (SecurityTokenException e) {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Gera um novo par de tokens de acesso e refresh com base no refresh token fornecido.
        /// </summary>
        /// <remarks>
        /// Exemplo de requisição:<br/>
        /// 
        ///     POST /api/Auth/refresh-token<br/>
        ///     {
        ///         // (O refresh token deve ser enviado como cookie)
        ///     }
        /// 
        /// </remarks>
        /// <returns>Retorna 200 (OK) com os novos tokens de acesso e refresh em cookies HTTP-only.</returns>
        /// <response code="200">Tokens renovados com sucesso.</response>
        /// <response code="401">
        /// Falha devido a:
        /// - Refresh token inválido
        /// - Sessão expirada
        /// - Token na blacklist
        /// </response>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                Request.Cookies.TryGetValue(REFRESH_TOKEN, out string? refreshToken);
                _logger.LogError("Token recebido" + refreshToken);
                if (string.IsNullOrEmpty(refreshToken)) {
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

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
                    _logger.LogError("Token não possui id da sessão.");
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                if (await _loggedUserService.IsTokenBlackListed(user.Id, idSessao, refreshToken)) {
                    _logger.LogError("O token utilizado já havia sido inválidado.");
                    return Unauthorized(ERROR_SESSAO_INVALIDA);
                }

                var userClaims = await _userManager.GetClaimsAsync(user);
                userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, idSessao));

                string newRefreshToken = GenerateRefreshToken(userClaims);
                string newAccessToken = GenerateAccessToken(userClaims);

                await _loggedUserService.UpdateSessionRefreshToken(user.Id, idSessao, refreshToken, newRefreshToken);

                AddTokensToCookies(newRefreshToken, newAccessToken);
                return Ok();
            }
            catch (BusinessException e)
            { 
                _logger.LogError(e, "RefreshToken não gerado: {Message}", e.Message);
                return Unauthorized(ERROR_SESSAO_INVALIDA);
            }
        }

        /// <summary>
        /// Valida o token de login e retorna o usuário associado se válido.
        /// </summary>
        /// <returns>
        /// Retorna o usuário autenticado se o token for válido,
        /// ou null se o token for inválido/inexistente.
        /// </returns>
        private async Task<ApplicationUser?> ValidateLoginToken()
        {
            Request.Cookies.TryGetValue(LOGIN_TOKEN, out string? loginToken);
            if (string.IsNullOrEmpty(loginToken)) {
                return null;
            }

            var principalLogin = await GetPrincipalFromLoginToken(loginToken);

            if (principalLogin.Identity == null || principalLogin.Identity.Name == null) {
                _logger.LogError("Não foi possível identificar a identidade do login token.");
                return null;
            }

            var user = await _userManager.FindByNameAsync(principalLogin.Identity.Name);
            if (user == null) {
                _logger.LogError("Usuário do login token não foi encontrado");
                return null;
            }

            return user;
        }
    }
}
