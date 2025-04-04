using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Models.Response;
using Auth_Turkeysoftware.Repositories.DataBaseModels;

namespace Auth_Turkeysoftware.Services
{
    public interface IUserSessionService
    {

        /// <summary>
        /// Adiciona a sessão de um usuário logado no banco de dados.
        /// </summary>
        /// <param name="loggedUserModel">Modelo contendo as informações da sessão do usuário.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        Task AddLoggedUser(UserSessionModel loggedUserModel);

        /// <summary>
        /// Invalida a sessão de um usuário.
        /// </summary>
        /// <param name="idUsuario">ID do usuário.</param>
        /// <param name="sessionId">ID da sessão.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        Task InvalidateUserSession(string idUsuario , string sessionId);

        /// <summary>
        /// Invalida todas as sessões de um usuário.
        /// </summary>
        /// <param name="userId">ID do usuário.</param>
        /// <param name="sessionId">ID da sessão.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        Task InvalidateAllUserSession(string userId, string sessionId);

        /// <summary>
        /// Verifica se o token do usuário está na lista negra.
        /// </summary>
        /// <param name="idUsuario">ID do usuário.</param>
        /// <param name="idSessao">ID da sessão.</param>
        /// <param name="userToken">Token do usuário.</param>
        /// <returns>Retorna verdadeiro se o token estiver na lista negra, caso contrário, falso.</returns>
        Task<bool> IsTokenBlackListed(string idUsuario, string idSessao, string userToken);

        /// <summary>
        /// Atualiza o refresh token da sessão.
        /// </summary>
        /// <param name="idUsuario">ID do usuário.</param>
        /// <param name="idSessao">ID da sessão.</param>
        /// <param name="refreshToken">Token de atualização atual.</param>
        /// <param name="newRefreshToken">Novo token de atualização.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        Task UpdateSessionRefreshToken(string idUsuario, string idSessao, string refreshToken, string newRefreshToken);

        /// <summary>
        /// Obtém a geolocalização pelo endereço IP.
        /// </summary>
        /// <param name="loggedUserModel">Modelo contendo as informações da sessão do usuário.</param>
        /// <returns>Retorna o modelo da sessão do usuário com a geolocalização atualizada.</returns>
        Task<UserSessionModel> GetGeolocationByIpAddress(UserSessionModel loggedUserModel);

        /// <summary>
        /// Lista as sessões ativas do usuário de forma paginada.
        /// </summary>
        /// <param name="userId">ID do usuário.</param>
        /// <param name="page">Número da página.</param>
        /// <returns>Retorna um DTO de paginação contendo as sessões ativas do usuário.</returns>
        Task<PaginationDTO<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int page);
    }
}
