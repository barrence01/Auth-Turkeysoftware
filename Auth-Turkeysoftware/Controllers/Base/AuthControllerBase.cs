using Auth_Turkeysoftware.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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

        protected JwtSecurityToken GenerateAccessToken(IList<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtAccessSecretKey()));

            var token = new JwtSecurityToken(
                issuer: GetJwtIssuer(),
                audience: GetJwtAudience(),
                expires: DateTime.Now.AddMinutes(GetAccessTokenValidityInMinutes()),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        protected JwtSecurityToken GenerateRefreshToken(IList<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtRefreshSecretKey()));

            var token = new JwtSecurityToken(
                issuer: GetJwtIssuer(),
                audience: GetJwtAudience(),
                expires: DateTime.Now.AddMinutes(GetRefreshTokenValidityInMinutes()),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
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
        protected void AddTokensToCookies(JwtSecurityToken refreshToken, JwtSecurityToken accessToken)
        {
            DeletePreviousTokenFromCookies();

            HttpContext.Response.Cookies.Append(REFRESH_TOKEN, new JwtSecurityTokenHandler().WriteToken(refreshToken),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = GetJwtDomain(),
                    Path = GetRefreshTokenPath(),
                    MaxAge = refreshToken.ValidTo.TimeOfDay
                });

            HttpContext.Response.Cookies.Append(ACCESS_TOKEN, new JwtSecurityTokenHandler().WriteToken(accessToken),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = GetJwtDomain(),
                    Path = GetAccessTokenPath(),
                    MaxAge = accessToken.ValidTo.TimeOfDay
                });
        }
        protected ClaimsPrincipal GetPrincipalFromRefreshToken(string? refreshToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = GetJwtAudience(),
                ValidateIssuer = true,
                ValidIssuer = GetJwtIssuer(),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtRefreshSecretKey())),
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
