using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace Auth_Turkeysoftware.Controllers.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
                                                    CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(new Response(
                                                    StatusCodes.Status500InternalServerError, "Error", "Internal Server error", exception.Message)), cancellationToken);

            return true;
        }
    }

    public class Response
    {
        public int Status { get; }
        public string Title { get; }
        public string Message { get; }
        public string? Errors { get; }

        public Response(int status, string title, string message, string? error)
        {
            Status = status;
            Title = title;
            Message = message;
            Errors = error;
        }
    }
}
