-- Create SecurityConfigurations table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SecurityConfigurations' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[SecurityConfigurations] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedBy] NVARCHAR(256) NOT NULL DEFAULT '',
        
        -- HTTPS Security
        [EnableHttpsRedirect] BIT NOT NULL DEFAULT 0,
        [EnableHsts] BIT NOT NULL DEFAULT 0,
        [HstsMaxAge] INT NOT NULL DEFAULT 31536000,
        [HstsIncludeSubDomains] BIT NOT NULL DEFAULT 1,
        [HstsPreload] BIT NOT NULL DEFAULT 0,
        
        -- Content Security Policy
        [EnableCsp] BIT NOT NULL DEFAULT 0,
        [CspDefaultSrc] NVARCHAR(500) NOT NULL DEFAULT '''self''',
        [CspScriptSrc] NVARCHAR(500) NOT NULL DEFAULT '''self'' ''unsafe-inline'' ''unsafe-eval''',
        [CspStyleSrc] NVARCHAR(500) NOT NULL DEFAULT '''self'' ''unsafe-inline''',
        [CspImgSrc] NVARCHAR(500) NOT NULL DEFAULT '''self'' data: https:',
        [CspConnectSrc] NVARCHAR(500) NOT NULL DEFAULT '''self''',
        [CspFontSrc] NVARCHAR(500) NOT NULL DEFAULT '''self''',
        [CspObjectSrc] NVARCHAR(500) NOT NULL DEFAULT '''none''',
        [CspMediaSrc] NVARCHAR(500) NOT NULL DEFAULT '''self''',
        [CspFrameSrc] NVARCHAR(500) NOT NULL DEFAULT '''none''',
        
        -- XSS Protection
        [EnableXssProtection] BIT NOT NULL DEFAULT 0,
        [EnableXssFilter] BIT NOT NULL DEFAULT 1,
        [EnableXssBlockMode] BIT NOT NULL DEFAULT 1,
        
        -- Frame Protection
        [EnableFrameOptions] BIT NOT NULL DEFAULT 0,
        [FrameOptions] NVARCHAR(50) NOT NULL DEFAULT 'DENY',
        
        -- Content Type Protection
        [EnableContentTypeOptions] BIT NOT NULL DEFAULT 0,
        
        -- Rate Limiting
        [EnableRateLimiting] BIT NOT NULL DEFAULT 0,
        [RateLimitRequestsPerMinute] INT NOT NULL DEFAULT 100,
        [RateLimitBurstSize] INT NOT NULL DEFAULT 200,
        [RateLimitAuthRequestsPerMinute] INT NOT NULL DEFAULT 10,
        [RateLimitAuthBurstSize] INT NOT NULL DEFAULT 20,
        
        -- CSRF Protection
        [EnableCsrfProtection] BIT NOT NULL DEFAULT 0,
        [CsrfHeaderName] NVARCHAR(100) NOT NULL DEFAULT 'X-CSRF-TOKEN',
        [CsrfCookieName] NVARCHAR(100) NOT NULL DEFAULT 'XSRF-TOKEN',
        
        -- JWT Security
        [EnableHttpOnlyCookies] BIT NOT NULL DEFAULT 0,
        [EnableSecureCookies] BIT NOT NULL DEFAULT 0,
        [EnableSameSiteStrict] BIT NOT NULL DEFAULT 0,
        [JwtExpirationMinutes] INT NOT NULL DEFAULT 60,
        [JwtRefreshExpirationDays] INT NOT NULL DEFAULT 7,
        
        -- Audit Logging
        [EnableAuditLogging] BIT NOT NULL DEFAULT 0,
        [LogFailedAuthAttempts] BIT NOT NULL DEFAULT 1,
        [LogSuccessfulAuthAttempts] BIT NOT NULL DEFAULT 1,
        [LogApiCalls] BIT NOT NULL DEFAULT 0,
        [LogSecurityEvents] BIT NOT NULL DEFAULT 1,
        
        -- IP Security
        [EnableIpWhitelist] BIT NOT NULL DEFAULT 0,
        [AllowedIpRanges] NVARCHAR(MAX) NOT NULL DEFAULT '',
        [EnableIpBlacklist] BIT NOT NULL DEFAULT 0,
        [BlockedIpRanges] NVARCHAR(MAX) NOT NULL DEFAULT '',
        
        -- Advanced Security
        [EnableInputSanitization] BIT NOT NULL DEFAULT 0,
        [EnableSqlInjectionProtection] BIT NOT NULL DEFAULT 0,
        [EnableRequestSizeLimit] BIT NOT NULL DEFAULT 0,
        [MaxRequestSizeBytes] BIGINT NOT NULL DEFAULT 104857600,
        
        -- Security Headers
        [EnableReferrerPolicy] BIT NOT NULL DEFAULT 0,
        [ReferrerPolicy] NVARCHAR(100) NOT NULL DEFAULT 'strict-origin-when-cross-origin',
        [EnablePermissionsPolicy] BIT NOT NULL DEFAULT 0,
        [PermissionsPolicy] NVARCHAR(500) NOT NULL DEFAULT 'geolocation=(), microphone=(), camera=()'
    );
    
    PRINT 'SecurityConfigurations table created successfully';
END
ELSE
BEGIN
    PRINT 'SecurityConfigurations table already exists';
END

-- Insert default security configuration if none exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[SecurityConfigurations])
BEGIN
    INSERT INTO [dbo].[SecurityConfigurations] (
        [UpdatedBy],
        [EnableHttpsRedirect],
        [EnableHsts],
        [EnableCsp],
        [EnableXssProtection],
        [EnableFrameOptions],
        [EnableContentTypeOptions],
        [EnableRateLimiting],
        [EnableCsrfProtection],
        [EnableHttpOnlyCookies],
        [EnableAuditLogging],
        [EnableInputSanitization],
        [EnableSqlInjectionProtection],
        [EnableReferrerPolicy],
        [EnablePermissionsPolicy]
    ) VALUES (
        'System',
        0, -- HTTPS Redirect (disabled by default for development)
        0, -- HSTS (disabled by default for development)
        0, -- CSP (disabled by default to avoid breaking existing functionality)
        0, -- XSS Protection (disabled by default)
        0, -- Frame Options (disabled by default)
        0, -- Content Type Options (disabled by default)
        0, -- Rate Limiting (disabled by default)
        0, -- CSRF Protection (disabled by default)
        0, -- HTTP Only Cookies (disabled by default)
        0, -- Audit Logging (disabled by default)
        0, -- Input Sanitization (disabled by default)
        0, -- SQL Injection Protection (disabled by default)
        0, -- Referrer Policy (disabled by default)
        0  -- Permissions Policy (disabled by default)
    );
    
    PRINT 'Default security configuration inserted';
END
ELSE
BEGIN
    PRINT 'Security configuration already exists';
END

-- Create index on UpdatedAt for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SecurityConfigurations_UpdatedAt' AND object_id = OBJECT_ID('[dbo].[SecurityConfigurations]'))
BEGIN
    CREATE INDEX [IX_SecurityConfigurations_UpdatedAt] ON [dbo].[SecurityConfigurations] ([UpdatedAt] DESC);
    PRINT 'Index IX_SecurityConfigurations_UpdatedAt created';
END

PRINT 'Security configuration migration completed successfully';
