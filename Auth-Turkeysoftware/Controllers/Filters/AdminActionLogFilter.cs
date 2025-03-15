using Auth_Turkeysoftware.Extensions;
using Auth_Turkeysoftware.Models.DataBaseModels;
using Auth_Turkeysoftware.Repositories.Context;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace Auth_Turkeysoftware.Controllers.Filters
{
    public class AdminActionLoggingFilterAsync : IAsyncActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AdminActionLoggingFilterAsync> _logger;

        public AdminActionLoggingFilterAsync(IHttpContextAccessor httpContextAccessor,
                                             AppDbContext dbContext,
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

            _logger.LogWarning("Operação administrativa realizada: {methodName} por {userName} com os argumentos {arguments}",
                                methodName, userName, arguments);

            var logEntry = new AdminActionLogModel(userName, methodName, arguments);
            logEntry.TruncateAllFields();

            _dbContext.AdminActionLog.Add(logEntry);
            await _dbContext.SaveChangesAsync();

            await next();
        }
    }
}
