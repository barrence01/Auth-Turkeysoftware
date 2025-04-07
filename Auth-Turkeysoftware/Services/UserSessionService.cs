using Auth_Turkeysoftware.Enums;
using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Repositories.DataBaseModels;
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

        /// <inheritdoc/>
        public async Task AddLoggedUser(UserSessionModel loggedUserModel)
        {
            await _userSessionRepository.AddLoggedUser(loggedUserModel);
        }

        /// <inheritdoc/>
        public async Task InvalidateUserSession(string userId, string idSessao)
        {
            await _userSessionRepository.InvalidateUserSession(userId, idSessao);
        }

        /// <inheritdoc/>
        public async Task InvalidateAllUserSession(string userId, string idSessao)
        {
            await _userSessionRepository.InvalidateUserSession(userId, idSessao);
        }

        /// <inheritdoc/>
        public async Task<bool> IsTokenBlackListed(string userId, string sessionId, string userToken)
        {
            var result = await _userSessionRepository.FindRefreshToken(userId, sessionId, userToken);
            if (result == null || result.TokenStatus == (char)StatusTokenEnum.INATIVO)
                return true;

            return false;
        }

        /// <inheritdoc/>
        public async Task UpdateSessionRefreshToken(string userId, string sessionId, string refreshToken, string newRefreshToken)
        {
            await _userSessionRepository.UpdateSessionRefreshToken(userId, sessionId, refreshToken, newRefreshToken);
        }

        /// <inheritdoc/>
        public async Task<PaginationDTO<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int page)
        {
            if (page <= 0)
                page = 1;

            return await _userSessionRepository.ListUserActiveSessionsPaginated(userId, page, 10);
        }

        /// <inheritdoc/>
        public async Task<UserSessionModel> GetGeolocationByIpAddress(UserSessionModel loggedUserModel)
        {
            //if (!string.IsNullOrWhiteSpace(loggedUserModel.IP))
            //{
            //    IpDetailsDTO ipDetailsModel = await _externalApiService.GetIpDetails(loggedUserModel.IP);
            //    if (ipDetailsModel != null && ipDetailsModel.Status == "success")
            //    {
            //        loggedUserModel.ServiceProvider = ipDetailsModel.Org;
            //        loggedUserModel.UF = ipDetailsModel.Region;
            //        loggedUserModel.Country = ipDetailsModel.Country;
            //    }
            //}

            return loggedUserModel;
        }
    }
}
