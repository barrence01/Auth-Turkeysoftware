using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;

namespace Auth_Turkeysoftware.API.Handlers
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
                case NotImplementedException:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "Not Implemented";
                    break;
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "Internal Server error";
                    break;
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(new Response(
                                                    title, exception.Message, Activity.Current?.Id)), cancellationToken);

            return true;
        }
    }

    public class Response
    {
        public string Title { get; }
        public string? Message { get; }
        public string? TraceId { get; }

        public Response(string title, string message, string? traceId)
        {
            Title = title;
            Message = message;
            TraceId = traceId;
        }
    }
}
