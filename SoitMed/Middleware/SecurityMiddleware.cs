using Microsoft.AspNetCore.Http;
using SoitMed.Models.Security;
using SoitMed.Services;
using System.Net;

namespace SoitMed.Middleware
{
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SecurityMiddleware(
            RequestDelegate next,
            ILogger<SecurityMiddleware> logger,
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
            var config = await securityService.GetCurrentConfigurationAsync();

            if (config != null)
            {
                await ApplySecurityHeadersAsync(context, config);
                await ApplyIpSecurityAsync(context, config);
                await ApplyRequestSizeLimitAsync(context, config);
            }

            await _next(context);
        }

        private async Task ApplySecurityHeadersAsync(HttpContext context, SecurityConfigurationDto config)
        {
            try
            {
                // HTTPS Redirect
                if (config.EnableHttpsRedirect && !context.Request.IsHttps)
                {
                    var httpsUrl = $"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
                    context.Response.Redirect(httpsUrl, true);
                    return;
                }

                // HTTP Strict Transport Security (HSTS)
                if (config.EnableHsts && context.Request.IsHttps)
                {
                    var hstsValue = $"max-age={config.HstsMaxAge}";
                    if (config.HstsIncludeSubDomains) hstsValue += "; includeSubDomains";
                    if (config.HstsPreload) hstsValue += "; preload";
                    
                    context.Response.Headers.Append("Strict-Transport-Security", hstsValue);
                }

                // Content Security Policy
                if (config.EnableCsp)
                {
                    var cspBuilder = new List<string>();
                    
                    if (!string.IsNullOrEmpty(config.CspDefaultSrc)) cspBuilder.Add($"default-src {config.CspDefaultSrc}");
                    if (!string.IsNullOrEmpty(config.CspScriptSrc)) cspBuilder.Add($"script-src {config.CspScriptSrc}");
                    if (!string.IsNullOrEmpty(config.CspStyleSrc)) cspBuilder.Add($"style-src {config.CspStyleSrc}");
                    if (!string.IsNullOrEmpty(config.CspImgSrc)) cspBuilder.Add($"img-src {config.CspImgSrc}");
                    if (!string.IsNullOrEmpty(config.CspConnectSrc)) cspBuilder.Add($"connect-src {config.CspConnectSrc}");
                    if (!string.IsNullOrEmpty(config.CspFontSrc)) cspBuilder.Add($"font-src {config.CspFontSrc}");
                    if (!string.IsNullOrEmpty(config.CspObjectSrc)) cspBuilder.Add($"object-src {config.CspObjectSrc}");
                    if (!string.IsNullOrEmpty(config.CspMediaSrc)) cspBuilder.Add($"media-src {config.CspMediaSrc}");
                    if (!string.IsNullOrEmpty(config.CspFrameSrc)) cspBuilder.Add($"frame-src {config.CspFrameSrc}");

                    if (cspBuilder.Any())
                    {
                        context.Response.Headers.Append("Content-Security-Policy", string.Join("; ", cspBuilder));
                    }
                }

                // XSS Protection
                if (config.EnableXssProtection)
                {
                    var xssValue = "1";
                    if (config.EnableXssFilter) xssValue += "; mode=block";
                    if (config.EnableXssBlockMode) xssValue += "; report=/xss-report";
                    
                    context.Response.Headers.Append("X-XSS-Protection", xssValue);
                }

                // Frame Options
                if (config.EnableFrameOptions)
                {
                    context.Response.Headers.Append("X-Frame-Options", config.FrameOptions);
                }

                // Content Type Options
                if (config.EnableContentTypeOptions)
                {
                    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                }

                // Referrer Policy
                if (config.EnableReferrerPolicy)
                {
                    context.Response.Headers.Append("Referrer-Policy", config.ReferrerPolicy);
                }

                // Permissions Policy
                if (config.EnablePermissionsPolicy)
                {
                    context.Response.Headers.Append("Permissions-Policy", config.PermissionsPolicy);
                }

                // Additional Security Headers
                context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
                context.Response.Headers.Append("X-Download-Options", "noopen");
                context.Response.Headers.Append("Server", string.Empty); // Hide server information
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying security headers");
            }
        }

        private async Task ApplyIpSecurityAsync(HttpContext context, SecurityConfigurationDto config)
        {
            try
            {
                var clientIp = GetClientIpAddress(context);
                
                // IP Blacklist check
                if (config.EnableIpBlacklist && !string.IsNullOrEmpty(config.BlockedIpRanges))
                {
                    var blockedRanges = ParseIpRanges(config.BlockedIpRanges);
                    if (IsIpInRanges(clientIp, blockedRanges))
                    {
                        _logger.LogWarning("Blocked IP {ClientIp} attempted to access {Path}", clientIp, context.Request.Path);
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsync("Access denied");
                        return;
                    }
                }

                // IP Whitelist check
                if (config.EnableIpWhitelist && !string.IsNullOrEmpty(config.AllowedIpRanges))
                {
                    var allowedRanges = ParseIpRanges(config.AllowedIpRanges);
                    if (!IsIpInRanges(clientIp, allowedRanges))
                    {
                        _logger.LogWarning("Non-whitelisted IP {ClientIp} attempted to access {Path}", clientIp, context.Request.Path);
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsync("Access denied");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying IP security");
            }
        }

        private async Task ApplyRequestSizeLimitAsync(HttpContext context, SecurityConfigurationDto config)
        {
            try
            {
                if (config.EnableRequestSizeLimit)
                {
                    context.Request.Headers.Append("X-Max-Request-Size", config.MaxRequestSizeBytes.ToString());
                    
                    // Note: Actual request size limiting should be handled by web server configuration
                    // This is just for informational purposes
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying request size limit");
            }
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

        private static List<string> ParseIpRanges(string ipRangesString)
        {
            var ranges = new List<string>();
            
            if (string.IsNullOrEmpty(ipRangesString))
                return ranges;

            var parts = ipRangesString.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    ranges.Add(trimmed);
                }
            }

            return ranges;
        }

        private static bool IsIpInRanges(string clientIp, List<string> ipRanges)
        {
            if (string.IsNullOrEmpty(clientIp) || !ipRanges.Any())
                return false;

            foreach (var range in ipRanges)
            {
                if (range.Contains('/'))
                {
                    // CIDR notation
                    if (IsIpInCidrRange(clientIp, range))
                        return true;
                }
                else
                {
                    // Exact IP match
                    if (clientIp.Equals(range, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        private static bool IsIpInCidrRange(string clientIp, string cidrRange)
        {
            try
            {
                var parts = cidrRange.Split('/');
                if (parts.Length != 2) return false;

                var networkAddress = IPAddress.Parse(parts[0]);
                var cidr = int.Parse(parts[1]);
                
                var clientAddress = IPAddress.Parse(clientIp);
                
                // Convert to bytes for comparison
                var networkBytes = networkAddress.GetAddressBytes();
                var clientBytes = clientAddress.GetAddressBytes();
                
                if (networkBytes.Length != clientBytes.Length) return false;
                
                var maskBytes = new byte[networkBytes.Length];
                for (int i = 0; i < cidr / 8; i++)
                {
                    maskBytes[i] = 255;
                }
                
                if (cidr % 8 != 0)
                {
                    maskBytes[cidr / 8] = (byte)(255 << (8 - (cidr % 8)));
                }
                
                for (int i = 0; i < networkBytes.Length; i++)
                {
                    if ((networkBytes[i] & maskBytes[i]) != (clientBytes[i] & maskBytes[i]))
                        return false;
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
