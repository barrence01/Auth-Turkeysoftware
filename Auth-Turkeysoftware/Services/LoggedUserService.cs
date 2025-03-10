using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Models;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Services.ExternalServices;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        public async Task AddLoggedUser(LoggedUserModel loggedUserModel)
        {
            await _loggedUserRepository.AddLoggedUser(loggedUserModel);
        }

        public async Task InvalidateUserSession(string idSessao, string idUsuario)
        {
            await _loggedUserRepository.InvalidateUserSessionByIdSessaoAndIdUsuario(idSessao, idUsuario);
        }

        public async Task<bool> IsTokenBlackListed(string userId, string sessionId, string userToken)
        {
            var result = await _loggedUserRepository.FindRefreshToken(userId, sessionId, userToken);
            if (result == null || result.TokenStatus == (char)StatusTokenEnum.INATIVO)
                return true;

            return false;
        }

        public async Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string refreshToken, string newRefreshToken)
        {
            await _loggedUserRepository.UpdateSessionRefreshToken(idUsuario, idSessao, refreshToken, newRefreshToken);
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

        public async Task<PaginationModel<List<UserSessionModel>>> GetUserActiveSessions(string UserId, int pagina)
        {
            if (pagina <= 0)
                pagina = 1;

            int qtdRegistrosPorPagina = 20;
            long totalRegistros = await _loggedUserRepository.GetUserActiveSessionsByUserIdCount(UserId);
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / (double)qtdRegistrosPorPagina);

            if (totalRegistros <= 0 || pagina > totalPaginas) {
                return new PaginationModel<List<UserSessionModel>>(totalRegistros, totalPaginas,
                                                                   pagina, qtdRegistrosPorPagina, []);
            }

            var sessions = await _loggedUserRepository.GetUserActiveSessionsByUserId(UserId, pagina, qtdRegistrosPorPagina);

            return new PaginationModel<List<UserSessionModel>>(totalRegistros, totalPaginas,
                                                               pagina, qtdRegistrosPorPagina, sessions);
        }
    }
}
