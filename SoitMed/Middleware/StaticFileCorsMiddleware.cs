using Microsoft.AspNetCore.Http;

namespace SoitMed.Middleware
{
    /// <summary>
    /// Middleware to add CORS headers to static file responses
    /// This ensures images and other static files can be loaded from frontend
    /// </summary>
    public class StaticFileCorsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StaticFileCorsMiddleware> _logger;

        public StaticFileCorsMiddleware(RequestDelegate next, ILogger<StaticFileCorsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if this is a static file request (images, etc.)
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            
            // Common static file extensions
            var staticFileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".pdf", ".css", ".js", ".ico", ".webp" };
            var isStaticFile = staticFileExtensions.Any(ext => path.EndsWith(ext));

            if (isStaticFile)
            {
                // Add CORS headers for static files
                var origin = context.Request.Headers["Origin"].ToString();
                
                // Allow localhost origins in development
                if (!string.IsNullOrEmpty(origin) && 
                    (origin.Contains("localhost") || origin.Contains("127.0.0.1")))
                {
                    context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
                    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
                    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");
                    context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
                }
                else if (string.IsNullOrEmpty(origin))
                {
                    // If no origin header, allow all (for same-origin requests)
                    context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                }

                // Handle preflight OPTIONS request
                if (context.Request.Method == "OPTIONS")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.CompleteAsync();
                    return;
                }
            }

            await _next(context);
        }
    }
}


