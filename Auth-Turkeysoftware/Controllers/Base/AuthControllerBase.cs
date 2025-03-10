using Auth_Turkeysoftware.Exceptions;
using Auth_Turkeysoftware.Models;
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
        protected readonly IConfiguration _configuration;

        protected AuthControllerBase(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected JwtSecurityToken GenerateAccessToken(IList<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(getAccessSecretKey()));

            bool result = int.TryParse(getTokenSettings("AccessTokenValidityInMinutes"), out int accessTokenValidityInMinutes);

            if (!result)
            {
                throw new BusinessRuleException("Não foi possível converter TokenValidityInMinutes para númerico.");
            }

            var token = new JwtSecurityToken(
                issuer: getTokenSettings("Issuer"),
                expires: DateTime.Now.AddMinutes(accessTokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        protected JwtSecurityToken GenerateRefreshToken(IList<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(getRefreshSecretKey()));

            bool result = int.TryParse(getTokenSettings("RefreshTokenValidityInMinutes"), out int refreshTokenValidityInMinutes);

            if (!result)
            {
                throw new BusinessRuleException("Não foi possível converter TokenValidityInMinutes para númerico.");
            }

            var token = new JwtSecurityToken(
                issuer: getTokenSettings("Issuer"),
                expires: DateTime.Now.AddMinutes(refreshTokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        protected string getTokenSettings(string key)
        {
            string? value = _configuration[string.Concat("JwtBearerToken:", key)];
            if (value == null)
                throw new BusinessRuleException("Não foi possível obter a configuração de token específicada.");
            return value;
        }

        protected string getAccessSecretKey()
        {
            string? secretKey = _configuration["JwtBearerToken:AccessSecretKey"];
            if (secretKey == null)
                throw new BusinessRuleException("Não foi possível obter a chave do token.");
            return secretKey;
        }

        protected string getRefreshSecretKey()
        {
            string? secretKey = _configuration["JwtBearerToken:RefreshSecretKey"];
            if (secretKey == null)
                throw new BusinessRuleException("Não foi possível obter a chave do token.");
            return secretKey;
        }
    }
}
