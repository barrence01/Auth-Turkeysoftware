using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories;
using Auth_Turkeysoftware.Repositories.Context;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Auth_Turkeysoftware.Services
{
    public class AdministrationService : IAdministrationService
    {
        private readonly IAdministrationRepository _administrationRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdministrationService(IAdministrationRepository administrationRepository,
                                     IHttpContextAccessor httpContextAccessor)
        {
            _administrationRepository = administrationRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task InvalidateAllUserSession(string userId)
        {
            await _administrationRepository.InvalidateAllUserSessionByEmail(userId);
            await AddToLog(nameof(InvalidateAllUserSession), userId);
        }
        public async Task AddToLog(string methodName, params object[] arguments)
        {
            string username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";

            string argumentsJson = JsonSerializer.Serialize(arguments);

            Log.Information("Invoking method: {MethodName} with arguments: {ArgumentsJson} for user: {UserName}",
                methodName, argumentsJson, username);

            await _administrationRepository.AddToLog(username, methodName, argumentsJson);
        }
    }
}
