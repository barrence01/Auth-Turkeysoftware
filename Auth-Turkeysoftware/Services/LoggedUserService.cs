using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Services.ExternalServices;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Auth_Turkeysoftware.Services
{
    public class LoggedUserService : ILoggedUserService
    {
        private readonly ILoggedUserRepository _loggedUserRepository;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IExternalApiService _externalApiService;

        public LoggedUserService(ILoggedUserRepository loggedUserRepository,
                                 UserManager<ApplicationUser> userManager,
                                 IExternalApiService externalApiService)
        {
            _loggedUserRepository = loggedUserRepository;
            _userManager = userManager;
            _externalApiService = externalApiService;
        }

        public async Task<bool> ValidateLogin(string email, string password) {

            var user = await _userManager.FindByNameAsync(email);

            return true;
        }

        public async Task AddLoggedUser(LoggedUserModel loggedUserModel)
        {
            await _loggedUserRepository.AddLoggedUser(loggedUserModel);
        }

        public async Task InvalidateUserSession(int idSessao, string idUsuario)
        {
            await _loggedUserRepository.UpdateTokenToBlackListByIdAndIdUsuario(idSessao, idUsuario);
        }

        public async Task<bool> IsBlackListed(string userId, string UserToken)
        {
            var result = await _loggedUserRepository.FindBlackListedTokenByUserIdAndToken(userId, UserToken);
            if (result != null)
                return true;

            return false;
        }

        public async Task UpdateSessionRefreshToken(string id, string refreshToken, string newRefreshToken)
        {
            await _loggedUserRepository.UpdateSessionRefreshToken(id, refreshToken, newRefreshToken);
        }

        public async Task<LoggedUserModel> AddIpAddressDetails(LoggedUserModel loggedUserModel)
        {
            if (!string.IsNullOrWhiteSpace(loggedUserModel.IP))
            {
                IpDetailsModel ipDetailsModel = await _externalApiService.GetIpDetails(loggedUserModel.IP);
                if (ipDetailsModel != null && ipDetailsModel.Status == "success")
                {
                    loggedUserModel.Provedora = ipDetailsModel.Org;
                    loggedUserModel.UF = ipDetailsModel.Region;
                    loggedUserModel.Pais = ipDetailsModel.Country;
                }
            }

            return loggedUserModel;
        }
    }
}
