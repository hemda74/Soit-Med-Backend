using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SoitMed.Models.Security;
using SoitMed.Models;

namespace SoitMed.Services
{
    public interface ISecurityConfigurationService
    {
        Task<SecurityConfigurationDto?> GetCurrentConfigurationAsync();
        Task<SecurityConfigurationDto> UpdateConfigurationAsync(SecurityConfigurationUpdateRequest request, string updatedBy);
        Task<SecurityStatusDto> GetSecurityStatusAsync();
        Task<bool> ResetToDefaultsAsync(string updatedBy);
        Task<bool> IsFeatureEnabledAsync(string featureName);
        Task<SecurityConfiguration> GetConfigurationEntityAsync();
    }

    public class SecurityConfigurationService : ISecurityConfigurationService
    {
        private readonly Context _context;
        private readonly ILogger<SecurityConfigurationService> _logger;
        private readonly IMemoryCache _cache;

        public SecurityConfigurationService(
            Context context,
            ILogger<SecurityConfigurationService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<SecurityConfigurationDto?> GetCurrentConfigurationAsync()
        {
            const string cacheKey = "security_configuration";
            
            if (_cache.TryGetValue(cacheKey, out SecurityConfigurationDto? cachedConfig))
            {
                return cachedConfig;
            }

            var config = await _context.SecurityConfigurations
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (config == null)
            {
                // Create default configuration
                config = await CreateDefaultConfigurationAsync();
            }

            var dto = MapToDto(config);
            
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(30));
            
            return dto;
        }

        public async Task<SecurityConfigurationDto> UpdateConfigurationAsync(
            SecurityConfigurationUpdateRequest request, 
            string updatedBy)
        {
            var config = await GetConfigurationEntityAsync();
            
            if (config == null)
            {
                config = new SecurityConfiguration();
                _context.SecurityConfigurations.Add(config);
            }

            // Update only the properties that are provided
            UpdateConfigurationProperties(config, request);
            config.UpdatedAt = DateTime.UtcNow;
            config.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();

            // Clear cache
            _cache.Remove("security_configuration");

            // Log security configuration change
            _logger.LogWarning("Security configuration updated by {UpdatedBy} at {UpdatedAt}", 
                updatedBy, DateTime.UtcNow);

            return MapToDto(config);
        }

        public async Task<SecurityStatusDto> GetSecurityStatusAsync()
        {
            var config = await GetCurrentConfigurationAsync();
            
            if (config == null)
            {
                return new SecurityStatusDto
                {
                    IsConfigured = false,
                    EnabledFeaturesCount = 0,
                    TotalFeaturesCount = GetTotalFeatureCount(),
                    SecurityScore = 0,
                    Recommendations = new List<string> { "Security configuration not found. Please configure security settings." },
                    LastUpdated = DateTime.MinValue,
                    LastUpdatedBy = "System"
                };
            }

            var enabledFeatures = GetEnabledFeatures(config);
            var disabledFeatures = GetDisabledFeatures(config);
            var recommendations = GetRecommendations(config);

            return new SecurityStatusDto
            {
                IsConfigured = true,
                EnabledFeaturesCount = enabledFeatures.Count,
                TotalFeaturesCount = GetTotalFeatureCount(),
                SecurityScore = CalculateSecurityScore(enabledFeatures.Count),
                EnabledFeatures = enabledFeatures,
                DisabledFeatures = disabledFeatures,
                Recommendations = recommendations,
                LastUpdated = config.UpdatedAt,
                LastUpdatedBy = config.UpdatedBy
            };
        }

        public async Task<bool> ResetToDefaultsAsync(string updatedBy)
        {
            try
            {
                var config = await GetConfigurationEntityAsync();
                
                if (config != null)
                {
                    _context.SecurityConfigurations.Remove(config);
                }

                await CreateDefaultConfigurationAsync();
                
                // Clear cache
                _cache.Remove("security_configuration");

                _logger.LogWarning("Security configuration reset to defaults by {UpdatedBy}", updatedBy);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting security configuration");
                return false;
            }
        }

        public async Task<bool> IsFeatureEnabledAsync(string featureName)
        {
            var config = await GetCurrentConfigurationAsync();
            
            if (config == null) return false;

            return featureName.ToLower() switch
            {
                "httpsredirect" => config.EnableHttpsRedirect,
                "hsts" => config.EnableHsts,
                "csp" => config.EnableCsp,
                "xssprotection" => config.EnableXssProtection,
                "frameoptions" => config.EnableFrameOptions,
                "contenttypeoptions" => config.EnableContentTypeOptions,
                "ratelimiting" => config.EnableRateLimiting,
                "csrfprotection" => config.EnableCsrfProtection,
                "httponlycookies" => config.EnableHttpOnlyCookies,
                "auditlogging" => config.EnableAuditLogging,
                "ipwhitelist" => config.EnableIpWhitelist,
                "ipblacklist" => config.EnableIpBlacklist,
                "inputsanitization" => config.EnableInputSanitization,
                "sqlinjectionprotection" => config.EnableSqlInjectionProtection,
                "referrerpolicy" => config.EnableReferrerPolicy,
                "permissionspolicy" => config.EnablePermissionsPolicy,
                _ => false
            };
        }

        public async Task<SecurityConfiguration> GetConfigurationEntityAsync()
        {
            return await _context.SecurityConfigurations
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<SecurityConfiguration> CreateDefaultConfigurationAsync()
        {
            var defaultConfig = new SecurityConfiguration();
            _context.SecurityConfigurations.Add(defaultConfig);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Default security configuration created");
            
            return defaultConfig;
        }

        private static void UpdateConfigurationProperties(
            SecurityConfiguration config, 
            SecurityConfigurationUpdateRequest request)
        {
            // HTTPS Security
            if (request.EnableHttpsRedirect.HasValue) config.EnableHttpsRedirect = request.EnableHttpsRedirect.Value;
            if (request.EnableHsts.HasValue) config.EnableHsts = request.EnableHsts.Value;
            if (request.HstsMaxAge.HasValue) config.HstsMaxAge = request.HstsMaxAge.Value;
            if (request.HstsIncludeSubDomains.HasValue) config.HstsIncludeSubDomains = request.HstsIncludeSubDomains.Value;
            if (request.HstsPreload.HasValue) config.HstsPreload = request.HstsPreload.Value;

            // Content Security Policy
            if (request.EnableCsp.HasValue) config.EnableCsp = request.EnableCsp.Value;
            if (request.CspDefaultSrc != null) config.CspDefaultSrc = request.CspDefaultSrc;
            if (request.CspScriptSrc != null) config.CspScriptSrc = request.CspScriptSrc;
            if (request.CspStyleSrc != null) config.CspStyleSrc = request.CspStyleSrc;
            if (request.CspImgSrc != null) config.CspImgSrc = request.CspImgSrc;
            if (request.CspConnectSrc != null) config.CspConnectSrc = request.CspConnectSrc;
            if (request.CspFontSrc != null) config.CspFontSrc = request.CspFontSrc;
            if (request.CspObjectSrc != null) config.CspObjectSrc = request.CspObjectSrc;
            if (request.CspMediaSrc != null) config.CspMediaSrc = request.CspMediaSrc;
            if (request.CspFrameSrc != null) config.CspFrameSrc = request.CspFrameSrc;

            // XSS Protection
            if (request.EnableXssProtection.HasValue) config.EnableXssProtection = request.EnableXssProtection.Value;
            if (request.EnableXssFilter.HasValue) config.EnableXssFilter = request.EnableXssFilter.Value;
            if (request.EnableXssBlockMode.HasValue) config.EnableXssBlockMode = request.EnableXssBlockMode.Value;

            // Frame Protection
            if (request.EnableFrameOptions.HasValue) config.EnableFrameOptions = request.EnableFrameOptions.Value;
            if (request.FrameOptions != null) config.FrameOptions = request.FrameOptions;

            // Content Type Protection
            if (request.EnableContentTypeOptions.HasValue) config.EnableContentTypeOptions = request.EnableContentTypeOptions.Value;

            // Rate Limiting
            if (request.EnableRateLimiting.HasValue) config.EnableRateLimiting = request.EnableRateLimiting.Value;
            if (request.RateLimitRequestsPerMinute.HasValue) config.RateLimitRequestsPerMinute = request.RateLimitRequestsPerMinute.Value;
            if (request.RateLimitBurstSize.HasValue) config.RateLimitBurstSize = request.RateLimitBurstSize.Value;
            if (request.RateLimitAuthRequestsPerMinute.HasValue) config.RateLimitAuthRequestsPerMinute = request.RateLimitAuthRequestsPerMinute.Value;
            if (request.RateLimitAuthBurstSize.HasValue) config.RateLimitAuthBurstSize = request.RateLimitAuthBurstSize.Value;

            // CSRF Protection
            if (request.EnableCsrfProtection.HasValue) config.EnableCsrfProtection = request.EnableCsrfProtection.Value;
            if (request.CsrfHeaderName != null) config.CsrfHeaderName = request.CsrfHeaderName;
            if (request.CsrfCookieName != null) config.CsrfCookieName = request.CsrfCookieName;

            // JWT Security
            if (request.EnableHttpOnlyCookies.HasValue) config.EnableHttpOnlyCookies = request.EnableHttpOnlyCookies.Value;
            if (request.EnableSecureCookies.HasValue) config.EnableSecureCookies = request.EnableSecureCookies.Value;
            if (request.EnableSameSiteStrict.HasValue) config.EnableSameSiteStrict = request.EnableSameSiteStrict.Value;
            if (request.JwtExpirationMinutes.HasValue) config.JwtExpirationMinutes = request.JwtExpirationMinutes.Value;
            if (request.JwtRefreshExpirationDays.HasValue) config.JwtRefreshExpirationDays = request.JwtRefreshExpirationDays.Value;

            // Audit Logging
            if (request.EnableAuditLogging.HasValue) config.EnableAuditLogging = request.EnableAuditLogging.Value;
            if (request.LogFailedAuthAttempts.HasValue) config.LogFailedAuthAttempts = request.LogFailedAuthAttempts.Value;
            if (request.LogSuccessfulAuthAttempts.HasValue) config.LogSuccessfulAuthAttempts = request.LogSuccessfulAuthAttempts.Value;
            if (request.LogApiCalls.HasValue) config.LogApiCalls = request.LogApiCalls.Value;
            if (request.LogSecurityEvents.HasValue) config.LogSecurityEvents = request.LogSecurityEvents.Value;

            // IP Security
            if (request.EnableIpWhitelist.HasValue) config.EnableIpWhitelist = request.EnableIpWhitelist.Value;
            if (request.AllowedIpRanges != null) config.AllowedIpRanges = request.AllowedIpRanges;
            if (request.EnableIpBlacklist.HasValue) config.EnableIpBlacklist = request.EnableIpBlacklist.Value;
            if (request.BlockedIpRanges != null) config.BlockedIpRanges = request.BlockedIpRanges;

            // Advanced Security
            if (request.EnableInputSanitization.HasValue) config.EnableInputSanitization = request.EnableInputSanitization.Value;
            if (request.EnableSqlInjectionProtection.HasValue) config.EnableSqlInjectionProtection = request.EnableSqlInjectionProtection.Value;
            if (request.EnableRequestSizeLimit.HasValue) config.EnableRequestSizeLimit = request.EnableRequestSizeLimit.Value;
            if (request.MaxRequestSizeBytes.HasValue) config.MaxRequestSizeBytes = request.MaxRequestSizeBytes.Value;

            // Security Headers
            if (request.EnableReferrerPolicy.HasValue) config.EnableReferrerPolicy = request.EnableReferrerPolicy.Value;
            if (request.ReferrerPolicy != null) config.ReferrerPolicy = request.ReferrerPolicy;
            if (request.EnablePermissionsPolicy.HasValue) config.EnablePermissionsPolicy = request.EnablePermissionsPolicy.Value;
            if (request.PermissionsPolicy != null) config.PermissionsPolicy = request.PermissionsPolicy;
        }

        private static SecurityConfigurationDto MapToDto(SecurityConfiguration config)
        {
            return new SecurityConfigurationDto
            {
                Id = config.Id,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt,
                UpdatedBy = config.UpdatedBy,

                // HTTPS Security
                EnableHttpsRedirect = config.EnableHttpsRedirect,
                EnableHsts = config.EnableHsts,
                HstsMaxAge = config.HstsMaxAge,
                HstsIncludeSubDomains = config.HstsIncludeSubDomains,
                HstsPreload = config.HstsPreload,

                // Content Security Policy
                EnableCsp = config.EnableCsp,
                CspDefaultSrc = config.CspDefaultSrc,
                CspScriptSrc = config.CspScriptSrc,
                CspStyleSrc = config.CspStyleSrc,
                CspImgSrc = config.CspImgSrc,
                CspConnectSrc = config.CspConnectSrc,
                CspFontSrc = config.CspFontSrc,
                CspObjectSrc = config.CspObjectSrc,
                CspMediaSrc = config.CspMediaSrc,
                CspFrameSrc = config.CspFrameSrc,

                // XSS Protection
                EnableXssProtection = config.EnableXssProtection,
                EnableXssFilter = config.EnableXssFilter,
                EnableXssBlockMode = config.EnableXssBlockMode,

                // Frame Protection
                EnableFrameOptions = config.EnableFrameOptions,
                FrameOptions = config.FrameOptions,

                // Content Type Protection
                EnableContentTypeOptions = config.EnableContentTypeOptions,

                // Rate Limiting
                EnableRateLimiting = config.EnableRateLimiting,
                RateLimitRequestsPerMinute = config.RateLimitRequestsPerMinute,
                RateLimitBurstSize = config.RateLimitBurstSize,
                RateLimitAuthRequestsPerMinute = config.RateLimitAuthRequestsPerMinute,
                RateLimitAuthBurstSize = config.RateLimitAuthBurstSize,

                // CSRF Protection
                EnableCsrfProtection = config.EnableCsrfProtection,
                CsrfHeaderName = config.CsrfHeaderName,
                CsrfCookieName = config.CsrfCookieName,

                // JWT Security
                EnableHttpOnlyCookies = config.EnableHttpOnlyCookies,
                EnableSecureCookies = config.EnableSecureCookies,
                EnableSameSiteStrict = config.EnableSameSiteStrict,
                JwtExpirationMinutes = config.JwtExpirationMinutes,
                JwtRefreshExpirationDays = config.JwtRefreshExpirationDays,

                // Audit Logging
                EnableAuditLogging = config.EnableAuditLogging,
                LogFailedAuthAttempts = config.LogFailedAuthAttempts,
                LogSuccessfulAuthAttempts = config.LogSuccessfulAuthAttempts,
                LogApiCalls = config.LogApiCalls,
                LogSecurityEvents = config.LogSecurityEvents,

                // IP Security
                EnableIpWhitelist = config.EnableIpWhitelist,
                AllowedIpRanges = config.AllowedIpRanges,
                EnableIpBlacklist = config.EnableIpBlacklist,
                BlockedIpRanges = config.BlockedIpRanges,

                // Advanced Security
                EnableInputSanitization = config.EnableInputSanitization,
                EnableSqlInjectionProtection = config.EnableSqlInjectionProtection,
                EnableRequestSizeLimit = config.EnableRequestSizeLimit,
                MaxRequestSizeBytes = config.MaxRequestSizeBytes,

                // Security Headers
                EnableReferrerPolicy = config.EnableReferrerPolicy,
                ReferrerPolicy = config.ReferrerPolicy,
                EnablePermissionsPolicy = config.EnablePermissionsPolicy,
                PermissionsPolicy = config.PermissionsPolicy
            };
        }

        private static List<string> GetEnabledFeatures(SecurityConfigurationDto config)
        {
            var features = new List<string>();

            if (config.EnableHttpsRedirect) features.Add("HTTPS Redirect");
            if (config.EnableHsts) features.Add("HTTP Strict Transport Security");
            if (config.EnableCsp) features.Add("Content Security Policy");
            if (config.EnableXssProtection) features.Add("XSS Protection");
            if (config.EnableFrameOptions) features.Add("Frame Options");
            if (config.EnableContentTypeOptions) features.Add("Content Type Options");
            if (config.EnableRateLimiting) features.Add("Rate Limiting");
            if (config.EnableCsrfProtection) features.Add("CSRF Protection");
            if (config.EnableHttpOnlyCookies) features.Add("HTTP Only Cookies");
            if (config.EnableAuditLogging) features.Add("Audit Logging");
            if (config.EnableIpWhitelist) features.Add("IP Whitelist");
            if (config.EnableIpBlacklist) features.Add("IP Blacklist");
            if (config.EnableInputSanitization) features.Add("Input Sanitization");
            if (config.EnableSqlInjectionProtection) features.Add("SQL Injection Protection");
            if (config.EnableReferrerPolicy) features.Add("Referrer Policy");
            if (config.EnablePermissionsPolicy) features.Add("Permissions Policy");

            return features;
        }

        private static List<string> GetDisabledFeatures(SecurityConfigurationDto config)
        {
            var allFeatures = GetAllFeatureNames();
            var enabledFeatures = GetEnabledFeatures(config);
            
            return allFeatures.Except(enabledFeatures).ToList();
        }

        private static List<string> GetAllFeatureNames()
        {
            return new List<string>
            {
                "HTTPS Redirect",
                "HTTP Strict Transport Security",
                "Content Security Policy",
                "XSS Protection",
                "Frame Options",
                "Content Type Options",
                "Rate Limiting",
                "CSRF Protection",
                "HTTP Only Cookies",
                "Audit Logging",
                "IP Whitelist",
                "IP Blacklist",
                "Input Sanitization",
                "SQL Injection Protection",
                "Referrer Policy",
                "Permissions Policy"
            };
        }

        private static int GetTotalFeatureCount()
        {
            return GetAllFeatureNames().Count;
        }

        private static double CalculateSecurityScore(int enabledFeaturesCount)
        {
            var totalFeatures = GetTotalFeatureCount();
            return Math.Round((double)enabledFeaturesCount / totalFeatures * 100, 1);
        }

        private static List<string> GetRecommendations(SecurityConfigurationDto config)
        {
            var recommendations = new List<string>();

            if (!config.EnableHttpsRedirect) recommendations.Add("Enable HTTPS Redirect for secure connections");
            if (!config.EnableHsts) recommendations.Add("Enable HTTP Strict Transport Security (HSTS)");
            if (!config.EnableCsp) recommendations.Add("Enable Content Security Policy to prevent XSS attacks");
            if (!config.EnableXssProtection) recommendations.Add("Enable XSS Protection headers");
            if (!config.EnableFrameOptions) recommendations.Add("Enable Frame Options to prevent clickjacking");
            if (!config.EnableRateLimiting) recommendations.Add("Enable Rate Limiting to prevent brute force attacks");
            if (!config.EnableCsrfProtection) recommendations.Add("Enable CSRF Protection for state-changing operations");
            if (!config.EnableHttpOnlyCookies) recommendations.Add("Enable HTTP Only Cookies for JWT tokens");
            if (!config.EnableAuditLogging) recommendations.Add("Enable Audit Logging for security monitoring");
            if (!config.EnableInputSanitization) recommendations.Add("Enable Input Sanitization to prevent injection attacks");

            return recommendations;
        }
    }
}
