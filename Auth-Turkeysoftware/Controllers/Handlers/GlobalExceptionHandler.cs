using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using System.Net.Sockets;
using System.Text.Json;

namespace Auth_Turkeysoftware.Controllers.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
                                                    CancellationToken cancellationToken)
        {
            int statusCode;
            string title;
            switch (exception)
            {
                case SocketException:
                case NpgsqlException:
                case TimeoutException:
                case RetryLimitExceededException:
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    title = "Service Unavailable";
                    break;
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "Internal Server error";
                    break;
            }
            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(new Response(
                                                    statusCode, "Error", title, exception.Message)), cancellationToken);

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
