using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.DbContext;
using Auth_Turkeysoftware.Infraestructure.Database.Postgresql.Entities;
using Auth_Turkeysoftware.Shared.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace Auth_Turkeysoftware.API.Filters
{
    public class AdminActionLoggingFilterAsync : IAsyncActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CacheDbContext _dbContext;
        private readonly ILogger<AdminActionLoggingFilterAsync> _logger;

        public AdminActionLoggingFilterAsync(IHttpContextAccessor httpContextAccessor,
                                             CacheDbContext dbContext,
                                             ILogger<AdminActionLoggingFilterAsync> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
            var methodName = context.ActionDescriptor.DisplayName ?? "Unknown";

            var arguments = context.ActionArguments.Any()
                            ? JsonSerializer.Serialize(context.ActionArguments)
                            : "No arguments";

            _logger.LogWarning("Operação administrativa realizada: {MethodName} por {UserName} com os argumentos {Arguments}",
                                methodName, userName, arguments);

            var logEntry = new LogAdminActionModel(userName, methodName, arguments);
            logEntry.TruncateAllFields();

            _dbContext.AdminActionLog.Add(logEntry);
            await _dbContext.SaveChangesAsync();

            await next();
        }
    }
}
