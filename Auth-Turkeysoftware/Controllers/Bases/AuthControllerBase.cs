using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;
using Auth_Turkeysoftware.Configurations.Services;

namespace Auth_Turkeysoftware.Controllers.Bases
{
    [ApiController]
    public class AuthControllerBase : CommonControllerBase
    {
        protected const string ACCESS_TOKEN = "TurkeySoftware-AccessToken";
        protected const string REFRESH_TOKEN = "TurkeySoftware-RefreshToken";
        protected readonly JwtSettingsSingleton _jwtSettings;

        protected AuthControllerBase(JwtSettingsSingleton jwtSettingsSingleton)
        {
            _jwtSettings = jwtSettingsSingleton;
        }

        protected string GenerateAccessToken(IList<Claim> authClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtAccessSecretKey()));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.GetAccessTokenValidityInMinutes()),
                Issuer = _jwtSettings.GetJwtIssuer(),
                Audience = _jwtSettings.GetJwtAudience(),
                SigningCredentials = signingCredentials
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(tokenDescriptor); ;
        }

        protected string GenerateRefreshToken(IList<Claim> authClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtRefreshSecretKey()));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.GetRefreshTokenValidityInMinutes()),
                Issuer = _jwtSettings.GetJwtIssuer(),
                Audience = _jwtSettings.GetJwtAudience(),
                SigningCredentials = signingCredentials
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(tokenDescriptor);
        }

        protected void DeletePreviousTokenFromCookies()
        {
            HttpContext.Response.Cookies.Delete(REFRESH_TOKEN,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = _jwtSettings.GetJwtDomain(),
                    Path = _jwtSettings.GetRefreshTokenPath()
                });

            HttpContext.Response.Cookies.Delete(ACCESS_TOKEN,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = _jwtSettings.GetJwtDomain(),
                    Path = _jwtSettings.GetAccessTokenPath()
                });
        }
        protected void AddTokensToCookies(string refreshToken, string accessToken)
        {
            DeletePreviousTokenFromCookies();

            HttpContext.Response.Cookies.Append(REFRESH_TOKEN, refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = _jwtSettings.GetJwtDomain(),
                    Path = _jwtSettings.GetRefreshTokenPath(),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.GetRefreshTokenValidityInMinutes())
                });

            HttpContext.Response.Cookies.Append(ACCESS_TOKEN, accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = _jwtSettings.GetJwtDomain(),
                    Path = _jwtSettings.GetAccessTokenPath(),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.GetAccessTokenValidityInMinutes())
                });
        }
        protected async Task<ClaimsPrincipal> GetPrincipalFromRefreshToken(string? refreshToken)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _jwtSettings.GetJwtAudience(),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.GetJwtIssuer(),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtRefreshSecretKey())),
                ValidateLifetime = true
            };

            var handler = new JsonWebTokenHandler();
            var result = await handler.ValidateTokenAsync(refreshToken, validationParameters);

            if (!result.IsValid)
                throw new SecurityTokenException("Invalid token.");

            return new ClaimsPrincipal(result.ClaimsIdentity);
        }
    }
}
