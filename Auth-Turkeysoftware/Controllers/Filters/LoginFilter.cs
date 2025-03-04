using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace Auth_Turkeysoftware.Controllers.Filters
{
    public class LoginFilter : ActionFilterAttribute
    {
        /// <summary>
        /// Asynchronously executes the action filter. Reads and parses the request body to get IP, Platform, and UserAgent.
        /// If values are not found in the body, falls back to headers or connection info.
        /// Stores the values in HttpContext.Items for later use in the controller.
        /// </summary>
        /// <param name="context">The context for the action.</param>
        /// <param name="next">The delegate to execute the next action filter or action.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            var (ip, platform, userAgent) = await GetValuesFromRequestBodyAsync(httpContext.Request);

            if (string.IsNullOrEmpty(ip))
            {
                ip = httpContext.Connection.RemoteIpAddress?.ToString();
            }
            if (string.IsNullOrEmpty(platform))
            {
                httpContext.Request.Headers.TryGetValue("sec-ch-ua-platform", out var platformHeader);
                platform = platformHeader.ToString();
            }
            if (string.IsNullOrEmpty(userAgent))
            {
                httpContext.Request.Headers.TryGetValue("user-agent", out var userAgentHeader);
                userAgent = userAgentHeader.ToString();
            }

            // If i's localhost set to a default
            if (ip == "127.0.0.1")
                ip = "179.117.69.197";

            httpContext.Items["IP"] = ip;
            httpContext.Items["Platform"] = platform;
            httpContext.Items["UserAgent"] = userAgent;

            await next();
        }

        /// <summary>
        /// Helper method to asynchronously read and parse the request body for IP, Platform, and UserAgent.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A tuple containing the IP, Platform, and UserAgent values.</returns>
        private async Task<(string ip, string platform, string userAgent)> GetValuesFromRequestBodyAsync(HttpRequest request)
        {
            // Allow the request body to be read multiple times
            request.EnableBuffering();

            string requestBody = null;

            // Read the body content as a string
            using (var reader = new StreamReader(request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0; // Reset the position for further processing
            }

            string ip = null;
            string platform = null;
            string userAgent = null;

            if (!string.IsNullOrEmpty(requestBody))
            {
                using (var doc = JsonDocument.Parse(requestBody))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("IP", out var ipProperty))
                    {
                        ip = ipProperty.GetString();
                    }

                    if (root.TryGetProperty("Platform", out var platformProperty))
                    {
                        platform = platformProperty.GetString();
                    }

                    if (root.TryGetProperty("UserAgent", out var userAgentProperty))
                    {
                        userAgent = userAgentProperty.GetString();
                    }
                }
            }

            return (ip, platform, userAgent);
        }

    }
}
