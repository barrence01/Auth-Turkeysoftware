using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Services.ExternalServices;

namespace Auth_Turkeysoftware.Services
{
    public class UserSessionService : IUserSessionService
    {
        private readonly IUserSessionRepository _userSessionRepository;

        private readonly IExternalApiService _externalApiService;

        public UserSessionService(IUserSessionRepository loggedUserRepository,
                                 IExternalApiService externalApiService)
        {
            _userSessionRepository = loggedUserRepository;
            _externalApiService = externalApiService;
        }

        public async Task AddLoggedUser(UserSessionModel loggedUserModel)
        {
            await _userSessionRepository.AddLoggedUser(loggedUserModel);
        }

        public async Task InvalidateUserSession(string idSessao, string idUsuario)
        {
            await _userSessionRepository.InvalidateUserSessionByIdSessaoAndIdUsuario(idSessao, idUsuario);
        }

        public async Task<bool> IsTokenBlackListed(string userId, string sessionId, string userToken)
        {
            var result = await _userSessionRepository.FindRefreshToken(userId, sessionId, userToken);
            if (result == null || result.TokenStatus == (char)StatusTokenEnum.INATIVO)
                return true;

            return false;
        }

        public async Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string refreshToken, string newRefreshToken)
        {
            await _userSessionRepository.UpdateSessionRefreshToken(idUsuario, idSessao, refreshToken, newRefreshToken);
        }

        public async Task<UserSessionModel> GetGeolocationByIpAddress(UserSessionModel loggedUserModel)
        {
            //if (!string.IsNullOrWhiteSpace(loggedUserModel.IP))
            //{
            //    IpDetailsDTO ipDetailsModel = await _externalApiService.GetIpDetails(loggedUserModel.IP);
            //    if (ipDetailsModel != null && ipDetailsModel.Status == "success")
            //    {
            //        loggedUserModel.Provedora = ipDetailsModel.Org;
            //        loggedUserModel.UF = ipDetailsModel.Region;
            //        loggedUserModel.Pais = ipDetailsModel.Country;
            //    }
            //}

            return loggedUserModel;
        }

        public async Task<PaginationDTO<List<UserSessionDTO>>> GetUserActiveSessions(string userId, int pagina)
        {
            if (pagina <= 0)
                pagina = 1;

            int qtdRegistrosPorPagina = 20;
            long totalRegistros = await _userSessionRepository.GetUserActiveSessionsByUserIdCount(userId);
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / (double)qtdRegistrosPorPagina);

            if (totalRegistros <= 0 || pagina > totalPaginas) {
                return new PaginationDTO<List<UserSessionDTO>>(totalRegistros, totalPaginas,
                                                                   pagina, qtdRegistrosPorPagina, []);
            }

            var sessions = await _userSessionRepository.GetUserActiveSessionsByUserId(userId, pagina, qtdRegistrosPorPagina);

            return new PaginationDTO<List<UserSessionDTO>>(totalRegistros, totalPaginas,
                                                               pagina, qtdRegistrosPorPagina, sessions);
        }
    }
}
