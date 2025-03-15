using Auth_Turkeysoftware.Models.DTOs;
using Auth_Turkeysoftware.Repositories;

namespace Auth_Turkeysoftware.Services
{
    public class AdministrationService : IAdministrationService
    {
        private readonly IAdministrationRepository _administrationRepository;

        private readonly IUserSessionRepository _userSessionRepository;

        public AdministrationService(IAdministrationRepository administrationRepository,
                                     IUserSessionRepository userSessionRepository)
        {
            _administrationRepository = administrationRepository;
            _userSessionRepository = userSessionRepository;
        }

        public async Task InvalidateUserSession(string userId, string userSessionId)
        {
            if (string.IsNullOrEmpty(userSessionId))
                await _administrationRepository.InvalidateAllUserSessionByEmail(userId);
            else
                await _userSessionRepository.InvalidateUserSessionByIdSessaoAndIdUsuario(userId, userSessionId);
        }

        public async Task<PaginationDTO<List<UserSessionDTO>>> GetUserAllSessions(string userId, int pagina)
        {
            if (pagina <= 0)
                pagina = 1;

            int qtdRegistrosPorPagina = 20;
            long totalRegistros = await _userSessionRepository.GetUserActiveSessionsByUserIdCount(userId);
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / (double)qtdRegistrosPorPagina);

            if (totalRegistros <= 0 || pagina > totalPaginas)
            {
                return new PaginationDTO<List<UserSessionDTO>>(totalRegistros, totalPaginas,
                                                                   pagina, qtdRegistrosPorPagina, []);
            }

            var sessions = await _userSessionRepository.GetUserActiveSessionsByUserId(userId, pagina, qtdRegistrosPorPagina);

            return new PaginationDTO<List<UserSessionDTO>>(totalRegistros, totalPaginas,
                                                               pagina, qtdRegistrosPorPagina, sessions);
        }
    }
}
