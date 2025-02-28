using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Models.Facade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth_Turkeysoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IUserBlackListService _userBlackListFacade;

        public AuthenticationController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IUserBlackListService userBlackListFacade)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _userBlackListFacade = userBlackListFacade;
        }

        [Authorize]
        [HttpPost]
        [Route("teste")]
        public async Task<IActionResult> teste() {
            var email2 = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            Console.WriteLine(email2);
            return Ok(email2);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var accessToken = CreateAccessToken(authClaims);
                var refreshToken = CreateRefreshToken(authClaims);

                HttpContext.Response.Cookies.Append("RefreshToken", new JwtSecurityTokenHandler().WriteToken(refreshToken),
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Strict,
                        Domain = "localhost",
                        Path = "/api/Authentication/refresh-access-token",
                        MaxAge = refreshToken.ValidTo.TimeOfDay
                    });

                HttpContext.Response.Cookies.Append("AccessToken", new JwtSecurityTokenHandler().WriteToken(accessToken),
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Strict,
                        Domain = "localhost",
                        MaxAge = accessToken.ValidTo.TimeOfDay
                    });

                return Ok();
            }
            return BadRequest("Email ou senha inválido!");
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);

            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "Usuário já existe!" });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                UserName = model.Email,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                Console.WriteLine(JsonConvert.SerializeObject(result));
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response { Status = "Error", Message = "Criação de usuário falhou!", Data = result.Errors });
            }

            await CheckAndInsertDefaultRoles();

            await _userManager.AddToRoleAsync(user, UserRolesEnum.Master.ToString());

            return Ok(new Response { Status = "Success", Message = "Usuário criado com sucesso!" });
        }

        [HttpPost]
        [Route("register-user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);

            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new Response { Status = "Error", Message = "Usuário já existe!" });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                UserName = model.Email,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                Console.WriteLine(JsonConvert.SerializeObject(result));
                return StatusCode(StatusCodes.Status400BadRequest,
                    new Response { Status = "Error", Message = "Criação de usuário falhou!", Data = result.Errors });
            }

            await CheckAndInsertDefaultRoles();

            await _userManager.AddToRoleAsync(user, UserRolesEnum.User.ToString());

            return Ok(new Response { Status = "Success", Message = "Usuário criado com sucesso!" });
        }

        [HttpPost]
        [Route("refresh-access-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                Request.Cookies.TryGetValue("RefreshToken", out string refreshToken);
                if (refreshToken == null) {
                    return Unauthorized("Token inválido.");
                }

                var principalRefresh = GetPrincipalFromExpiredRefreshToken(refreshToken);

                string username = principalRefresh.Identity.Name;

                var user = await _userManager.FindByNameAsync(username);

                if (await _userBlackListFacade.IsBlackListed(user.UserName, refreshToken)) {
                    return Unauthorized("Token inválido.");
                }

                var newAccessToken = CreateAccessToken(principalRefresh.Claims.ToList());

                HttpContext.Response.Cookies.Append("AccessToken", new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Strict,
                        Domain = "localhost",
                        MaxAge = newAccessToken.ValidTo.TimeOfDay
                    });

                return Ok();
            } catch(Exception e) {
                Console.WriteLine(e.Message);
                return Unauthorized("Token inválido.");
            }
        }

        [Authorize]
        [HttpPost]
        [Route("revoke/{email}")]
        public async Task<IActionResult> Revoke()
        {
            Request.Cookies.TryGetValue("RefreshToken", out string refreshToken);
            if (refreshToken == null)
            {
                return Unauthorized("Token inválido.");
            }

            var userEmail = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null) {
                return BadRequest("Email inválido.");
            }
            else if (user.UserName != userEmail)
            {
                return Unauthorized("Não foi possivel revogar o token.");
            }

            await _userBlackListFacade.AddTokenInBlackList(new UserBlackListModel
            {
                EmailUsuario = user.UserName,
                RefreshToken = refreshToken
            });

            return NoContent();
        }

        private JwtSecurityToken CreateAccessToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(getAccessSecretKey()));

            bool result = int.TryParse(getTokenSettings("AccessTokenValidityInMinutes"), out int accessTokenValidityInMinutes);

            if (!result) {
                throw new BusinessRuleException("Não foi possível converter TokenValidityInMinutes para númerico.");
            }

            var token = new JwtSecurityToken(
                issuer: getTokenSettings("Issuer"),
                audience: "Audience",
                expires: DateTime.Now.AddMinutes(accessTokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private JwtSecurityToken CreateRefreshToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(getRefreshSecretKey()));

            bool result = int.TryParse(getTokenSettings("RefreshTokenValidityInMinutes"), out int refreshTokenValidityInMinutes);

            if (!result)
            {
                throw new BusinessRuleException("Não foi possível converter TokenValidityInMinutes para númerico.");
            }

            var token = new JwtSecurityToken(
                issuer: getTokenSettings("Issuer"),
                audience: "Audience",
                expires: DateTime.Now.AddMinutes(refreshTokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredRefreshToken(string? refreshToken)
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

        private async Task<bool> CheckAndInsertDefaultRoles()
        {
            foreach (string userRole in Enum.GetNames(typeof(UserRolesEnum))) {
                if (!await _roleManager.RoleExistsAsync(userRole.ToString()))
                    await _roleManager.CreateAsync(new IdentityRole(userRole.ToString()));
            }
            return true;
        }

        private string getTokenSettings(string key) {
            string? value = _configuration[string.Concat("JwtBearerToken:", key)];
            if (value == null)
                throw new BusinessRuleException("Não foi possível obter a configuração de token específicada.");
            return value;
        }
        private string getAccessSecretKey()
        {
            string? secretKey = _configuration["JwtBearerToken:AccessSecretKey"];
            if (secretKey == null)
                throw new BusinessRuleException("Não foi possível obter a chave do token.");
            return secretKey;
        }

        private string getRefreshSecretKey()
        {
            string? secretKey = _configuration["JwtBearerToken:RefreshSecretKey"];
            if (secretKey == null)
                throw new BusinessRuleException("Não foi possível obter a chave do token.");
            return secretKey;
        }
    }
}
