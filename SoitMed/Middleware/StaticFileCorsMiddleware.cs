using Microsoft.AspNetCore.Http;

namespace SoitMed.Middleware
{
    /// <summary>
    /// Ensures CORS headers are emitted for static file requests so that mobile/web clients
    /// can safely access voice message blobs and other assets served from wwwroot.
    /// </summary>
    public class StaticFileCorsMiddleware
    {
        private static readonly string[] _staticExtensions = new[]
        {
            ".jpg", ".jpeg", ".png", ".gif", ".svg", ".css", ".js", ".json",
            ".mp3", ".wav", ".m4a", ".aac", ".ogg", ".webm", ".mp4"
        };

        private readonly RequestDelegate _next;

        public StaticFileCorsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            var isStaticRequest = IsStaticFileRequest(path);

            if (isStaticRequest &&
                HttpMethods.IsOptions(context.Request.Method))
            {
                SetCorsHeaders(context.Response);
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            if (isStaticRequest)
            {
                context.Response.OnStarting(() =>
                {
                    SetCorsHeaders(context.Response);
                    return Task.CompletedTask;
                });
            }

            await _next(context);
        }

        private static bool IsStaticFileRequest(PathString path)
        {
            var value = path.Value;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (var ext in _staticExtensions)
            {
                if (value.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetCorsHeaders(HttpResponse response)
        {
            response.Headers["Access-Control-Allow-Origin"] = response.Headers.ContainsKey("Access-Control-Allow-Origin")
                ? response.Headers["Access-Control-Allow-Origin"].ToString()
                : "*";

            response.Headers["Access-Control-Allow-Methods"] = "GET,HEAD,OPTIONS";
            response.Headers["Access-Control-Allow-Headers"] = "Origin, X-Requested-With, Content-Type, Accept, Authorization";
            response.Headers["Access-Control-Expose-Headers"] = "Content-Length, Content-Type";
        }
    }
}

