# 🔒 Security Configuration System

A comprehensive, configurable security system for the Soit-Med application that allows administrators to toggle security features on/off through a user-friendly dashboard.

## 🚀 Features

### 🔐 **Configurable Security Features**
- **HTTPS Redirect** - Automatically redirect HTTP to HTTPS
- **HTTP Strict Transport Security (HSTS)** - Enforce secure connections
- **Content Security Policy (CSP)** - Prevent XSS attacks with configurable policies
- **XSS Protection** - Browser-based XSS filtering
- **Frame Options** - Prevent clickjacking attacks
- **Content Type Options** - Prevent MIME-type sniffing attacks
- **Rate Limiting** - Protect against brute force and DDoS attacks
- **CSRF Protection** - Prevent Cross-Site Request Forgery attacks
- **HTTP Only Cookies** - Secure JWT token storage
- **Audit Logging** - Comprehensive security event logging
- **IP Whitelist/Blacklist** - Control access by IP address
- **Input Sanitization** - Prevent injection attacks
- **SQL Injection Protection** - Database-level protection
- **Security Headers** - Additional security headers (Referrer Policy, Permissions Policy)

### 🎛️ **Security Dashboard**
- **Real-time Security Score** - Visual representation of security posture
- **Feature Toggle Interface** - Easy on/off switches for each security feature
- **Security Recommendations** - Automated suggestions for improvement
- **Configuration History** - Track who changed what and when
- **Test Tools** - Built-in security header testing

## 📋 Installation & Setup

### 1. Database Setup
Run the migration script to create the security configuration table:

```sql
-- Execute this script in your database
-- Path: Backend/Scripts/CreateSecurityConfigurationTable.sql
```

### 2. Backend Configuration
The security services are automatically registered in `Program.cs`. No additional configuration needed.

### 3. Frontend Access
Navigate to `/Admin/security` in your application (requires SuperAdmin or Admin role).

## 🔧 Usage

### Accessing the Security Dashboard
1. Log in as SuperAdmin or Admin
2. Navigate to **Admin** → **Security** (or go to `/Admin/security`)
3. Use the dashboard to configure security features

### Toggle Security Features
Each security feature can be safely toggled on/off using the switches:

1. **Basic Features Tab** - Core security protections
2. **Headers Tab** - HTTP security headers configuration  
3. **Advanced Tab** - Advanced security options
4. **Overview Tab** - Security status and recommendations

### Security Features Explained

#### 🔒 **HTTPS Security**
- **HTTPS Redirect**: Automatically redirects HTTP requests to HTTPS
- **HSTS**: Enforces HTTPS connections with configurable max-age and preload options

#### 🛡️ **Content Security Policy (CSP)**
- Prevents XSS attacks by controlling which resources can be loaded
- Configurable directives for scripts, styles, images, etc.
- Default policy allows same-origin resources with some inline exceptions

#### ⚡ **Rate Limiting**
- **General Rate Limiting**: 100 requests/minute with 200 burst capacity
- **Auth Rate Limiting**: 10 requests/minute with 20 burst capacity for authentication endpoints
- Automatic rate limit headers included in responses

#### 🌐 **IP Security**
- **IP Whitelist**: Only allow connections from specified IP ranges
- **IP Blacklist**: Block connections from specified IP ranges
- Supports CIDR notation (e.g., 192.168.1.0/24)

#### 📝 **Audit Logging**
- Logs failed authentication attempts
- Logs successful authentication attempts
- Logs API calls (optional)
- Logs security events

## 🔍 Security Score Calculation

The security score is calculated as:
```
Security Score = (Enabled Features / Total Features) × 100
```

**Score Interpretation:**
- 🟢 **80-100%**: Excellent security posture
- 🟡 **60-79%**: Good security with room for improvement  
- 🔴 **Below 60%**: Security needs attention

## 🚨 Important Safety Features

### ⚠️ **Safe Implementation**
- All security features are **disabled by default** to prevent breaking existing functionality
- Features can be toggled individually to test compatibility
- Automatic rollback if issues occur
- Comprehensive logging of all security configuration changes

### 🔄 **Fail-Safe Design**
- Security middleware fails open (allows requests) if errors occur
- Rate limiting fails open to prevent service disruption
- Configuration cached for performance with automatic refresh

### 👥 **Role-Based Access**
- Only **SuperAdmin** and **Admin** roles can access security dashboard
- All configuration changes are logged with user attribution
- Audit trail for compliance requirements

## 🛠️ Advanced Configuration

### Custom Rate Limiting
```csharp
// Custom rate limits per endpoint
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("CustomPolicy", context =>
    {
        // Custom rate limiting logic
    });
});
```

### Custom CSP Directives
The CSP policy can be customized through the dashboard or programmatically:

```javascript
// Example CSP configuration
{
    "default-src": "'self'",
    "script-src": "'self' 'unsafe-inline' https://trusted.cdn.com",
    "style-src": "'self' 'unsafe-inline'",
    "img-src": "'self' data: https:",
    "connect-src": "'self' https://api.example.com"
}
```

### IP Range Configuration
IP ranges support CIDR notation:
- Single IP: `192.168.1.100`
- Range: `192.168.1.0/24`
- Multiple ranges: `192.168.1.0/24,10.0.0.0/8`

## 📊 Monitoring & Testing

### Security Headers Test
Use the built-in test endpoint:
```
GET /api/security/test
```

This returns:
- Current security headers
- Client information
- Request details
- Security configuration status

### Rate Limiting Headers
All responses include rate limiting information:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 2024-01-01T12:00:00Z
```

## 🔧 Troubleshooting

### Common Issues

#### **Features Not Working**
1. Check if the feature is enabled in the dashboard
2. Verify the middleware order in `Program.cs`
3. Check application logs for errors

#### **Rate Limiting Too Aggressive**
1. Adjust limits in the dashboard
2. Increase burst size for traffic spikes
3. Add IP whitelist for trusted sources

#### **CSP Breaking Functionality**
1. Start with restrictive policy and gradually allow more resources
2. Use browser developer tools to identify blocked resources
3. Add specific domains to CSP directives

#### **Performance Issues**
1. Security middleware is optimized for performance
2. Caching reduces database calls
3. Rate limiting uses in-memory counters

### Debug Mode
Enable detailed logging in development:
```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

## 📋 Security Best Practices

### 🎯 **Recommended Configuration**
1. **Production Environment**:
   - ✅ Enable HTTPS Redirect
   - ✅ Enable HSTS
   - ✅ Enable CSP
   - ✅ Enable Rate Limiting
   - ✅ Enable Audit Logging
   - ✅ Enable Security Headers

2. **Development Environment**:
   - ❌ Disable HTTPS Redirect (for local testing)
   - ❌ Disable HSTS (for local testing)
   - ⚠️ Use permissive CSP for development

### 🔒 **Security Checklist**
- [ ] Enable HTTPS in production
- [ ] Configure appropriate CSP policy
- [ ] Set reasonable rate limits
- [ ] Enable audit logging
- [ ] Monitor security events
- [ ] Regular security reviews
- [ ] Test security configurations

## 🔄 Updates & Maintenance

### Database Migration
When updating security features, run the latest migration script to ensure database schema is current.

### Configuration Backup
Regular backup of security configuration is recommended:
```sql
-- Backup security configuration
SELECT * INTO SecurityConfigurations_Backup 
FROM SecurityConfigurations;
```

## 📞 Support

For security-related issues:
1. Check application logs
2. Test with security features disabled
3. Verify configuration in dashboard
4. Review this documentation

## 🏷️ Tags

`security` `configuration` `middleware` `rate-limiting` `csp` `https` `authentication` `authorization` `audit-logging`
