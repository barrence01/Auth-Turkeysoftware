using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;

namespace Auth_Turkeysoftware.Controllers.Base
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
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtAccessSecretKey()));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(GetAccessTokenValidityInMinutes()),
                Issuer = GetJwtIssuer(),
                Audience = GetJwtAudience(),
                SigningCredentials = signingCredentials
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(tokenDescriptor); ;
        }

        protected string GenerateRefreshToken(IList<Claim> authClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtRefreshSecretKey()));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(GetRefreshTokenValidityInMinutes()),
                Issuer = GetJwtIssuer(),
                Audience = GetJwtAudience(),
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
                    Domain = GetJwtDomain(),
                    Path = GetRefreshTokenPath()
                });

            HttpContext.Response.Cookies.Delete(ACCESS_TOKEN,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = GetJwtDomain(),
                    Path = GetAccessTokenPath()
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
                    Domain = GetJwtDomain(),
                    Path = GetRefreshTokenPath(),
                    Expires = DateTime.UtcNow.AddMinutes(GetRefreshTokenValidityInMinutes())
                });

            HttpContext.Response.Cookies.Append(ACCESS_TOKEN, accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = GetJwtDomain(),
                    Path = GetAccessTokenPath(),
                    Expires = DateTime.UtcNow.AddMinutes(GetAccessTokenValidityInMinutes())
                });
        }
        protected async Task<ClaimsPrincipal> GetPrincipalFromRefreshToken(string? refreshToken)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = GetJwtAudience(),
                ValidateIssuer = true,
                ValidIssuer = GetJwtIssuer(),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtRefreshSecretKey())),
                ValidateLifetime = true
            };

            var handler = new JsonWebTokenHandler();
            var result = await handler.ValidateTokenAsync(refreshToken, validationParameters);

            if (!result.IsValid)
                throw new SecurityTokenException("Invalid token.");

            return new ClaimsPrincipal(result.ClaimsIdentity);
        }

        private string GetJwtAccessSecretKey()
        {
            return _jwtSettings.GetJwtSettings().AccessSecretKey;
        }

        private string GetJwtRefreshSecretKey()
        {
            return _jwtSettings.GetJwtSettings().RefreshSecretKey;
        }

        protected string GetJwtDomain()
        {
            return _jwtSettings.GetJwtSettings().Domain;
        }

        protected string GetJwtIssuer()
        {
            return _jwtSettings.GetJwtSettings().Domain;
        }

        protected string GetJwtAudience()
        {
            return _jwtSettings.GetJwtSettings().Domain;
        }

        protected int GetRefreshTokenValidityInMinutes()
        {
            return _jwtSettings.GetJwtSettings().RefreshTokenValidityInMinutes;
        }

        protected int GetAccessTokenValidityInMinutes()
        {
            return _jwtSettings.GetJwtSettings().AccessTokenValidityInMinutes;
        }
        protected string GetRefreshTokenPath()
        {
            return _jwtSettings.GetJwtSettings().RefreshTokenPath;
        }
        protected string GetAccessTokenPath()
        {
            return _jwtSettings.GetJwtSettings().AccessTokenPath;
        }
    }
}
