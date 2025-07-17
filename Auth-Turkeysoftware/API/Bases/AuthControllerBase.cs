using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;
using Auth_Turkeysoftware.Infraestructure.Configurations.Singletons;
using Auth_Turkeysoftware.Shared.Constants;

namespace Auth_Turkeysoftware.API.Bases
{
    [ApiController]
    public class AuthControllerBase : CommonControllerBase
    {
        protected readonly JwtSettingsSingleton _jwtSettings;

        protected AuthControllerBase(JwtSettingsSingleton jwtSettingsSingleton)
        {
            _jwtSettings = jwtSettingsSingleton;
        }

        protected string GenerateLoginToken(IList<Claim> authClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtLoginSecretKey()));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var encryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtEncryptionKey()));
            var encryptionCredentials = new EncryptingCredentials(encryptionKey, SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.GetAccessTokenValidityInMinutes()),
                Issuer = _jwtSettings.GetJwtIssuer(),
                Audience = _jwtSettings.GetJwtAudience(),
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptionCredentials
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(tokenDescriptor);
        }

        protected string GenerateAccessToken(IList<Claim> authClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtAccessSecretKey()));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var encryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtEncryptionKey()));
            var encryptionCredentials = new EncryptingCredentials(encryptionKey, SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.GetAccessTokenValidityInMinutes()),
                Issuer = _jwtSettings.GetJwtIssuer(),
                Audience = _jwtSettings.GetJwtAudience(),
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptionCredentials
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(tokenDescriptor); ;
        }

        protected string GenerateRefreshToken(IList<Claim> authClaims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtRefreshSecretKey()));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var encryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtEncryptionKey()));
            var encryptionCredentials = new EncryptingCredentials(encryptionKey, SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.GetRefreshTokenValidityInMinutes()),
                Issuer = _jwtSettings.GetJwtIssuer(),
                Audience = _jwtSettings.GetJwtAudience(),
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptionCredentials
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(tokenDescriptor);
        }

        protected void AddLoginTokenToCookies(string loginToken)
        {
            HttpContext.Response.Cookies.Append(TokenNameConstant.LOGIN_TOKEN, loginToken,
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

        protected void AddTokensToCookies(string refreshToken, string accessToken)
        {
            DeletePreviousTokenFromCookies();

            HttpContext.Response.Cookies.Append(TokenNameConstant.REFRESH_TOKEN, refreshToken,
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

            HttpContext.Response.Cookies.Append(TokenNameConstant.ACCESS_TOKEN, accessToken,
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
                ValidateLifetime = true,
                TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtEncryptionKey()))
            };

            var handler = new JsonWebTokenHandler();
            var result = await handler.ValidateTokenAsync(refreshToken, validationParameters);

            if (!result.IsValid)
                throw new SecurityTokenException("Invalid token.");

            return new ClaimsPrincipal(result.ClaimsIdentity);
        }

        protected async Task<ClaimsPrincipal> GetPrincipalFromLoginToken(string? loginToken)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _jwtSettings.GetJwtAudience(),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.GetJwtIssuer(),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtLoginSecretKey())),
                ValidateLifetime = true,
                TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.GetJwtEncryptionKey()))
            };

            var handler = new JsonWebTokenHandler();
            var result = await handler.ValidateTokenAsync(loginToken, validationParameters);

            if (!result.IsValid)
                throw new SecurityTokenException("Invalid token.");

            return new ClaimsPrincipal(result.ClaimsIdentity);
        }

        protected void DeletePreviousTokenFromCookies()
        {
            HttpContext.Response.Cookies.Delete(TokenNameConstant.REFRESH_TOKEN,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = _jwtSettings.GetJwtDomain(),
                    Path = _jwtSettings.GetRefreshTokenPath()
                });

            HttpContext.Response.Cookies.Delete(TokenNameConstant.ACCESS_TOKEN,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None,
                    Domain = _jwtSettings.GetJwtDomain(),
                    Path = _jwtSettings.GetAccessTokenPath()
                });

            HttpContext.Response.Cookies.Delete(TokenNameConstant.LOGIN_TOKEN,
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
    }
}
