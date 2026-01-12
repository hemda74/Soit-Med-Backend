using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Models.Security;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityConfigurationService _securityService;
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(
            ISecurityConfigurationService securityService,
            ILogger<SecurityController> logger)
        {
            _securityService = securityService;
            _logger = logger;
        }

        [HttpGet("configuration")]
        public async Task<ActionResult<SecurityConfigurationDto>> GetConfiguration()
        {
            try
            {
                var config = await _securityService.GetCurrentConfigurationAsync();
                
                if (config == null)
                {
                    return NotFound("Security configuration not found");
                }

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security configuration");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("status")]
        public async Task<ActionResult<SecurityStatusDto>> GetSecurityStatus()
        {
            try
            {
                var status = await _securityService.GetSecurityStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security status");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("configuration")]
        public async Task<ActionResult<SecurityConfigurationDto>> UpdateConfiguration(
            [FromBody] SecurityConfigurationUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedBy = User.Identity?.Name ?? "Unknown";
                var config = await _securityService.UpdateConfigurationAsync(request, updatedBy);

                _logger.LogWarning("Security configuration updated by {UpdatedBy}", updatedBy);

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security configuration");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("reset")]
        public async Task<ActionResult> ResetToDefaults()
        {
            try
            {
                var updatedBy = User.Identity?.Name ?? "Unknown";
                var success = await _securityService.ResetToDefaultsAsync(updatedBy);

                if (success)
                {
                    _logger.LogWarning("Security configuration reset to defaults by {UpdatedBy}", updatedBy);
                    return Ok("Security configuration reset to defaults");
                }
                else
                {
                    return BadRequest("Failed to reset security configuration");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting security configuration");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("feature/{featureName}/enabled")]
        public async Task<ActionResult<bool>> IsFeatureEnabled(string featureName)
        {
            try
            {
                var isEnabled = await _securityService.IsFeatureEnabledAsync(featureName);
                return Ok(isEnabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if feature {FeatureName} is enabled", featureName);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("feature/{featureName}/toggle")]
        public async Task<ActionResult<bool>> ToggleFeature(string featureName, [FromBody] bool enabled)
        {
            try
            {
                var currentConfig = await _securityService.GetCurrentConfigurationAsync();
                if (currentConfig == null)
                {
                    return NotFound("Security configuration not found");
                }

                var updateRequest = new SecurityConfigurationUpdateRequest
                {
                    Id = currentConfig.Id
                };

                // Set the appropriate property based on feature name
                switch (featureName.ToLower())
                {
                    case "httpsredirect":
                        updateRequest.EnableHttpsRedirect = enabled;
                        break;
                    case "hsts":
                        updateRequest.EnableHsts = enabled;
                        break;
                    case "csp":
                        updateRequest.EnableCsp = enabled;
                        break;
                    case "xssprotection":
                        updateRequest.EnableXssProtection = enabled;
                        break;
                    case "frameoptions":
                        updateRequest.EnableFrameOptions = enabled;
                        break;
                    case "contenttypeoptions":
                        updateRequest.EnableContentTypeOptions = enabled;
                        break;
                    case "ratelimiting":
                        updateRequest.EnableRateLimiting = enabled;
                        break;
                    case "csrfprotection":
                        updateRequest.EnableCsrfProtection = enabled;
                        break;
                    case "httponlycookies":
                        updateRequest.EnableHttpOnlyCookies = enabled;
                        break;
                    case "auditlogging":
                        updateRequest.EnableAuditLogging = enabled;
                        break;
                    case "ipwhitelist":
                        updateRequest.EnableIpWhitelist = enabled;
                        break;
                    case "ipblacklist":
                        updateRequest.EnableIpBlacklist = enabled;
                        break;
                    case "inputsanitization":
                        updateRequest.EnableInputSanitization = enabled;
                        break;
                    case "sqlinjectionprotection":
                        updateRequest.EnableSqlInjectionProtection = enabled;
                        break;
                    case "referrerpolicy":
                        updateRequest.EnableReferrerPolicy = enabled;
                        break;
                    case "permissionspolicy":
                        updateRequest.EnablePermissionsPolicy = enabled;
                        break;
                    default:
                        return BadRequest($"Unknown feature: {featureName}");
                }

                var updatedBy = User.Identity?.Name ?? "Unknown";
                var config = await _securityService.UpdateConfigurationAsync(updateRequest, updatedBy);

                _logger.LogWarning("Security feature {FeatureName} toggled to {Enabled} by {UpdatedBy}", 
                    featureName, enabled, updatedBy);

                return Ok(enabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling feature {FeatureName}", featureName);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("test")]
        public async Task<ActionResult<object>> TestSecurityHeaders()
        {
            try
            {
                var headers = new Dictionary<string, string>();
                
                foreach (var header in HttpContext.Response.Headers)
                {
                    headers[header.Key] = header.Value;
                }

                return Ok(new
                {
                    Message = "Security headers test endpoint",
                    Timestamp = DateTime.UtcNow,
                    Headers = headers,
                    ClientIp = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    IsSecure = Request.IsHttps,
                    Path = Request.Path,
                    Method = Request.Method
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing security headers");
                return StatusCode(500, "Internal server error");
            }
        }

        private string GetClientIpAddress()
        {
            // This would typically be injected via HttpContext
            return HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
