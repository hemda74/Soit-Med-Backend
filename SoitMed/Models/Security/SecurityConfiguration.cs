using System.ComponentModel.DataAnnotations;

namespace SoitMed.Models.Security
{
    public class SecurityConfiguration
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = string.Empty;

        // HTTPS Security
        public bool EnableHttpsRedirect { get; set; } = false;
        public bool EnableHsts { get; set; } = false;
        public int HstsMaxAge { get; set; } = 31536000; // 1 year
        public bool HstsIncludeSubDomains { get; set; } = true;
        public bool HstsPreload { get; set; } = false;

        // Content Security Policy
        public bool EnableCsp { get; set; } = false;
        public string CspDefaultSrc { get; set; } = "'self'";
        public string CspScriptSrc { get; set; } = "'self' 'unsafe-inline' 'unsafe-eval'";
        public string CspStyleSrc { get; set; } = "'self' 'unsafe-inline'";
        public string CspImgSrc { get; set; } = "'self' data: https:";
        public string CspConnectSrc { get; set; } = "'self'";
        public string CspFontSrc { get; set; } = "'self'";
        public string CspObjectSrc { get; set; } = "'none'";
        public string CspMediaSrc { get; set; } = "'self'";
        public string CspFrameSrc { get; set; } = "'none'";

        // XSS Protection
        public bool EnableXssProtection { get; set; } = false;
        public bool EnableXssFilter { get; set; } = true;
        public bool EnableXssBlockMode { get; set; } = true;

        // Frame Protection
        public bool EnableFrameOptions { get; set; } = false;
        public string FrameOptions { get; set; } = "DENY";

        // Content Type Protection
        public bool EnableContentTypeOptions { get; set; } = false;

        // Rate Limiting
        public bool EnableRateLimiting { get; set; } = false;
        public int RateLimitRequestsPerMinute { get; set; } = 100;
        public int RateLimitBurstSize { get; set; } = 200;
        public int RateLimitAuthRequestsPerMinute { get; set; } = 10;
        public int RateLimitAuthBurstSize { get; set; } = 20;

        // CSRF Protection
        public bool EnableCsrfProtection { get; set; } = false;
        public string CsrfHeaderName { get; set; } = "X-CSRF-TOKEN";
        public string CsrfCookieName { get; set; } = "XSRF-TOKEN";

        // JWT Security
        public bool EnableHttpOnlyCookies { get; set; } = false;
        public bool EnableSecureCookies { get; set; } = false;
        public bool EnableSameSiteStrict { get; set; } = false;
        public int JwtExpirationMinutes { get; set; } = 60;
        public int JwtRefreshExpirationDays { get; set; } = 7;

        // Audit Logging
        public bool EnableAuditLogging { get; set; } = false;
        public bool LogFailedAuthAttempts { get; set; } = true;
        public bool LogSuccessfulAuthAttempts { get; set; } = true;
        public bool LogApiCalls { get; set; } = false;
        public bool LogSecurityEvents { get; set; } = true;

        // IP Security
        public bool EnableIpWhitelist { get; set; } = false;
        public string AllowedIpRanges { get; set; } = string.Empty;
        public bool EnableIpBlacklist { get; set; } = false;
        public string BlockedIpRanges { get; set; } = string.Empty;

        // Advanced Security
        public bool EnableInputSanitization { get; set; } = false;
        public bool EnableSqlInjectionProtection { get; set; } = false;
        public bool EnableRequestSizeLimit { get; set; } = false;
        public long MaxRequestSizeBytes { get; set; } = 104857600; // 100MB

        // Security Headers
        public bool EnableReferrerPolicy { get; set; } = false;
        public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";
        public bool EnablePermissionsPolicy { get; set; } = false;
        public string PermissionsPolicy { get; set; } = "geolocation=(), microphone=(), camera=()";
    }

    public class SecurityConfigurationUpdateRequest
    {
        [Required]
        public int Id { get; set; }

        // HTTPS Security
        public bool? EnableHttpsRedirect { get; set; }
        public bool? EnableHsts { get; set; }
        public int? HstsMaxAge { get; set; }
        public bool? HstsIncludeSubDomains { get; set; }
        public bool? HstsPreload { get; set; }

        // Content Security Policy
        public bool? EnableCsp { get; set; }
        public string? CspDefaultSrc { get; set; }
        public string? CspScriptSrc { get; set; }
        public string? CspStyleSrc { get; set; }
        public string? CspImgSrc { get; set; }
        public string? CspConnectSrc { get; set; }
        public string? CspFontSrc { get; set; }
        public string? CspObjectSrc { get; set; }
        public string? CspMediaSrc { get; set; }
        public string? CspFrameSrc { get; set; }

        // XSS Protection
        public bool? EnableXssProtection { get; set; }
        public bool? EnableXssFilter { get; set; }
        public bool? EnableXssBlockMode { get; set; }

        // Frame Protection
        public bool? EnableFrameOptions { get; set; }
        public string? FrameOptions { get; set; }

        // Content Type Protection
        public bool? EnableContentTypeOptions { get; set; }

        // Rate Limiting
        public bool? EnableRateLimiting { get; set; }
        public int? RateLimitRequestsPerMinute { get; set; }
        public int? RateLimitBurstSize { get; set; }
        public int? RateLimitAuthRequestsPerMinute { get; set; }
        public int? RateLimitAuthBurstSize { get; set; }

        // CSRF Protection
        public bool? EnableCsrfProtection { get; set; }
        public string? CsrfHeaderName { get; set; }
        public string? CsrfCookieName { get; set; }

        // JWT Security
        public bool? EnableHttpOnlyCookies { get; set; }
        public bool? EnableSecureCookies { get; set; }
        public bool? EnableSameSiteStrict { get; set; }
        public int? JwtExpirationMinutes { get; set; }
        public int? JwtRefreshExpirationDays { get; set; }

        // Audit Logging
        public bool? EnableAuditLogging { get; set; }
        public bool? LogFailedAuthAttempts { get; set; }
        public bool? LogSuccessfulAuthAttempts { get; set; }
        public bool? LogApiCalls { get; set; }
        public bool? LogSecurityEvents { get; set; }

        // IP Security
        public bool? EnableIpWhitelist { get; set; }
        public string? AllowedIpRanges { get; set; }
        public bool? EnableIpBlacklist { get; set; }
        public string? BlockedIpRanges { get; set; }

        // Advanced Security
        public bool? EnableInputSanitization { get; set; }
        public bool? EnableSqlInjectionProtection { get; set; }
        public bool? EnableRequestSizeLimit { get; set; }
        public long? MaxRequestSizeBytes { get; set; }

        // Security Headers
        public bool? EnableReferrerPolicy { get; set; }
        public string? ReferrerPolicy { get; set; }
        public bool? EnablePermissionsPolicy { get; set; }
        public string? PermissionsPolicy { get; set; }
    }

    public class SecurityConfigurationDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;

        // HTTPS Security
        public bool EnableHttpsRedirect { get; set; }
        public bool EnableHsts { get; set; }
        public int HstsMaxAge { get; set; }
        public bool HstsIncludeSubDomains { get; set; }
        public bool HstsPreload { get; set; }

        // Content Security Policy
        public bool EnableCsp { get; set; }
        public string CspDefaultSrc { get; set; } = string.Empty;
        public string CspScriptSrc { get; set; } = string.Empty;
        public string CspStyleSrc { get; set; } = string.Empty;
        public string CspImgSrc { get; set; } = string.Empty;
        public string CspConnectSrc { get; set; } = string.Empty;
        public string CspFontSrc { get; set; } = string.Empty;
        public string CspObjectSrc { get; set; } = string.Empty;
        public string CspMediaSrc { get; set; } = string.Empty;
        public string CspFrameSrc { get; set; } = string.Empty;

        // XSS Protection
        public bool EnableXssProtection { get; set; }
        public bool EnableXssFilter { get; set; }
        public bool EnableXssBlockMode { get; set; }

        // Frame Protection
        public bool EnableFrameOptions { get; set; }
        public string FrameOptions { get; set; } = string.Empty;

        // Content Type Protection
        public bool EnableContentTypeOptions { get; set; }

        // Rate Limiting
        public bool EnableRateLimiting { get; set; }
        public int RateLimitRequestsPerMinute { get; set; }
        public int RateLimitBurstSize { get; set; }
        public int RateLimitAuthRequestsPerMinute { get; set; }
        public int RateLimitAuthBurstSize { get; set; }

        // CSRF Protection
        public bool EnableCsrfProtection { get; set; }
        public string CsrfHeaderName { get; set; } = string.Empty;
        public string CsrfCookieName { get; set; } = string.Empty;

        // JWT Security
        public bool EnableHttpOnlyCookies { get; set; }
        public bool EnableSecureCookies { get; set; }
        public bool EnableSameSiteStrict { get; set; }
        public int JwtExpirationMinutes { get; set; }
        public int JwtRefreshExpirationDays { get; set; }

        // Audit Logging
        public bool EnableAuditLogging { get; set; }
        public bool LogFailedAuthAttempts { get; set; }
        public bool LogSuccessfulAuthAttempts { get; set; }
        public bool LogApiCalls { get; set; }
        public bool LogSecurityEvents { get; set; }

        // IP Security
        public bool EnableIpWhitelist { get; set; }
        public string AllowedIpRanges { get; set; } = string.Empty;
        public bool EnableIpBlacklist { get; set; }
        public string BlockedIpRanges { get; set; } = string.Empty;

        // Advanced Security
        public bool EnableInputSanitization { get; set; }
        public bool EnableSqlInjectionProtection { get; set; }
        public bool EnableRequestSizeLimit { get; set; }
        public long MaxRequestSizeBytes { get; set; }

        // Security Headers
        public bool EnableReferrerPolicy { get; set; }
        public string ReferrerPolicy { get; set; } = string.Empty;
        public bool EnablePermissionsPolicy { get; set; }
        public string PermissionsPolicy { get; set; } = string.Empty;
    }

    public class SecurityStatusDto
    {
        public bool IsConfigured { get; set; }
        public int EnabledFeaturesCount { get; set; }
        public int TotalFeaturesCount { get; set; }
        public double SecurityScore { get; set; }
        public List<string> EnabledFeatures { get; set; } = new();
        public List<string> DisabledFeatures { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; } = string.Empty;
    }
}
