using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.API.Models.Response;
using Auth_Turkeysoftware.Domain.Models.VOs;

namespace Auth_Turkeysoftware.Domain.Services.Interfaces
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
        /// <param name="userId">ID do usuário.</param>
        /// <param name="sessionId">ID da sessão.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        Task InvalidateUserSession(string userId, string sessionId);

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
        /// <param name="userId">ID do usuário.</param>
        /// <param name="sessionId">ID da sessão.</param>
        /// <param name="userToken">Token do usuário.</param>
        /// <returns>Retorna verdadeiro se o token estiver na lista negra, caso contrário, falso.</returns>
        Task<bool> IsTokenBlackListed(string userId, string sessionId, string userToken);

        /// <summary>
        /// Atualiza o refresh token da sessão.
        /// </summary>
        /// <param name="userId">ID do usuário.</param>
        /// <param name="sessionId">ID da sessão.</param>
        /// <param name="refreshToken">Token de atualização atual.</param>
        /// <param name="newRefreshToken">Novo token de atualização.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        Task UpdateSessionRefreshToken(string userId, string sessionId, string refreshToken, string newRefreshToken);

        /// <summary>
        /// Lista as sessões ativas do usuário de forma paginada.
        /// </summary>
        /// <param name="userId">ID do usuário.</param>
        /// <param name="pageNumber">Número da página.</param>
        /// <returns>Retorna um DTO de paginação contendo as sessões ativas do usuário.</returns>
        Task<PaginationVO<UserSessionResponse>> ListUserActiveSessionsPaginated(string userId, int pageNumber);
    }
}
