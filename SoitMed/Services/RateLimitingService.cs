using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace SoitMed.Services
{
    public interface IRateLimitingService
    {
        Task<bool> IsAllowedAsync(string key, int requestsPerMinute, int burstSize);
        Task<bool> IsAuthAllowedAsync(string key);
        Task ResetAsync(string key);
        Task<RateLimitInfo> GetRateLimitInfoAsync(string key);
    }

    public class RateLimitInfo
    {
        public bool IsAllowed { get; set; }
        public int RemainingRequests { get; set; }
        public int TotalRequests { get; set; }
        public TimeSpan ResetTime { get; set; }
        public DateTime NextResetTime { get; set; }
    }

    public class RateLimitingService : IRateLimitingService
    {
        private readonly ILogger<RateLimitingService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentDictionary<string, RateLimitCounter> _counters;

        public RateLimitingService(
            ILogger<RateLimitingService> logger,
            IMemoryCache cache,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _cache = cache;
            _scopeFactory = scopeFactory;
            _counters = new ConcurrentDictionary<string, RateLimitCounter>();
        }

        public async Task<bool> IsAllowedAsync(string key, int requestsPerMinute, int burstSize)
        {
            try
            {
                var counter = _counters.GetOrAdd(key, _ => new RateLimitCounter());
                
                lock (counter)
                {
                    var now = DateTime.UtcNow;
                    
                    // Reset if window has expired
                    if (now >= counter.WindowStart.AddMinutes(1))
                    {
                        counter.Count = 0;
                        counter.WindowStart = now;
                    }

                    // Check if under the rate limit
                    if (counter.Count < requestsPerMinute)
                    {
                        counter.Count++;
                        return true;
                    }

                    // Check burst allowance
                    if (counter.BurstCount < burstSize)
                    {
                        counter.BurstCount++;
                        _logger.LogWarning("Rate limit burst used for key: {Key}", key);
                        return true;
                    }

                    _logger.LogWarning("Rate limit exceeded for key: {Key}", key);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for key: {Key}", key);
                // Fail open - allow the request if there's an error
                return true;
            }
        }

        public async Task<bool> IsAuthAllowedAsync(string key)
        {
            using var scope = _scopeFactory.CreateScope();
            var securityService = scope.ServiceProvider.GetRequiredService<ISecurityConfigurationService>();
            var config = await securityService.GetCurrentConfigurationAsync();

            if (config?.EnableRateLimiting != true)
            {
                return true; // Rate limiting disabled
            }

            return await IsAllowedAsync(
                $"auth_{key}", 
                config.RateLimitAuthRequestsPerMinute, 
                config.RateLimitAuthBurstSize);
        }

        public async Task ResetAsync(string key)
        {
            try
            {
                if (_counters.TryGetValue(key, out var counter))
                {
                    lock (counter)
                    {
                        counter.Count = 0;
                        counter.BurstCount = 0;
                        counter.WindowStart = DateTime.UtcNow;
                    }
                    
                    _logger.LogInformation("Rate limit reset for key: {Key}", key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting rate limit for key: {Key}", key);
            }
        }

        public async Task<RateLimitInfo> GetRateLimitInfoAsync(string key)
        {
            try
            {
                var counter = _counters.GetOrAdd(key, _ => new RateLimitCounter());
                
                lock (counter)
                {
                    var now = DateTime.UtcNow;
                    var windowEnd = counter.WindowStart.AddMinutes(1);
                    var timeUntilReset = windowEnd > now ? windowEnd - now : TimeSpan.Zero;

                    return new RateLimitInfo
                    {
                        IsAllowed = counter.Count < 100, // Default limit
                        RemainingRequests = Math.Max(0, 100 - counter.Count),
                        TotalRequests = counter.Count,
                        ResetTime = timeUntilReset,
                        NextResetTime = windowEnd
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rate limit info for key: {Key}", key);
                return new RateLimitInfo
                {
                    IsAllowed = true,
                    RemainingRequests = 100,
                    TotalRequests = 0,
                    ResetTime = TimeSpan.FromMinutes(1),
                    NextResetTime = DateTime.UtcNow.AddMinutes(1)
                };
            }
        }
    }

    public class RateLimitCounter
    {
        public int Count { get; set; }
        public int BurstCount { get; set; }
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
    }

    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var securityService = scope.ServiceProvider.GetRequiredService<ISecurityConfigurationService>();
            var rateLimitingService = scope.ServiceProvider.GetRequiredService<IRateLimitingService>();
            var config = await securityService.GetCurrentConfigurationAsync();

            if (config?.EnableRateLimiting == true)
            {
                var key = GetClientKey(context);
                var isAuthEndpoint = IsAuthenticationEndpoint(context.Request.Path);

                bool isAllowed;
                if (isAuthEndpoint)
                {
                    isAllowed = await rateLimitingService.IsAuthAllowedAsync(key);
                }
                else
                {
                    isAllowed = await rateLimitingService.IsAllowedAsync(
                        key, 
                        config.RateLimitRequestsPerMinute, 
                        config.RateLimitBurstSize);
                }

                if (!isAllowed)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers.Append("Retry-After", "60");
                    context.Response.Headers.Append("X-RateLimit-Limit", config.RateLimitRequestsPerMinute.ToString());
                    
                    var rateLimitInfo = await rateLimitingService.GetRateLimitInfoAsync(key);
                    context.Response.Headers.Append("X-RateLimit-Remaining", rateLimitInfo.RemainingRequests.ToString());
                    context.Response.Headers.Append("X-RateLimit-Reset", rateLimitInfo.NextResetTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));

                    await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                    return;
                }

                // Add rate limit headers to successful responses
                var rateLimitInfoResponse = await rateLimitingService.GetRateLimitInfoAsync(key);
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Append("X-RateLimit-Limit", config.RateLimitRequestsPerMinute.ToString());
                    context.Response.Headers.Append("X-RateLimit-Remaining", rateLimitInfoResponse.RemainingRequests.ToString());
                    context.Response.Headers.Append("X-RateLimit-Reset", rateLimitInfoResponse.NextResetTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    return Task.CompletedTask;
                });
            }

            await _next(context);
        }

        private static string GetClientKey(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var userAgentHash = userAgent.GetHashCode().ToString("X");
            
            return $"{clientIp}:{userAgentHash}";
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            
            // Check for forwarded headers (load balancers, proxies)
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedIps = context.Request.Headers["X-Forwarded-For"].ToString().Split(',');
                if (forwardedIps.Length > 0)
                {
                    ipAddress = forwardedIps[0].Trim();
                }
            }
            else if (context.Request.Headers.ContainsKey("X-Real-IP"))
            {
                ipAddress = context.Request.Headers["X-Real-IP"].ToString();
            }

            return ipAddress ?? "unknown";
        }

        private static bool IsAuthenticationEndpoint(string path)
        {
            var authPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register", 
                "/api/auth/forgot-password",
                "/api/auth/reset-password",
                "/api/auth/verify-code",
                "/api/auth/refresh-token"
            };

            return authPaths.Any(authPath => path.StartsWith(authPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}
