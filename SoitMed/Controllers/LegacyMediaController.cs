using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Services;
using Microsoft.Extensions.Configuration;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Controller for serving legacy media files (proxy to legacy media API)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LegacyMediaController : ControllerBase
    {
        private readonly ILegacyMediaService _legacyMediaService;
        private readonly ILogger<LegacyMediaController> _logger;

        public LegacyMediaController(
            ILegacyMediaService legacyMediaService,
            ILogger<LegacyMediaController> logger)
        {
            _legacyMediaService = legacyMediaService;
            _logger = logger;
        }

        /// <summary>
        /// Get legacy media file (proxy to legacy media API)
        /// GET /api/LegacyMedia/files/{fileName}
        /// </summary>
        [HttpGet("files/{fileName}")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Encoding")]
        public async Task<IActionResult> GetMediaFile(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "File name is required" });
            }

            try
            {
                // Security: Prevent path traversal
                if (fileName.Contains("..") || Path.IsPathRooted(fileName))
                {
                    _logger.LogWarning("Invalid file name detected (path traversal attempt): {FileName}", fileName);
                    return BadRequest(new { message = "Invalid file name" });
                }

                // URL decode the filename in case it was encoded
                fileName = Uri.UnescapeDataString(fileName);

                _logger.LogInformation("Requesting media file: {FileName}", fileName);

                // Try to get file stream first (more reliable)
                var stream = await _legacyMediaService.GetMediaFileStreamAsync(fileName, cancellationToken);
                if (stream == null)
                {
                    _logger.LogWarning("Could not retrieve file stream: {FileName}", fileName);
                    return NotFound(new { message = $"File '{fileName}' could not be retrieved" });
                }

                // Get file info for metadata (if available)
                var fileInfo = await _legacyMediaService.GetMediaFileInfoAsync(fileName, cancellationToken);
                var contentType = fileInfo?.ContentType ?? "application/octet-stream";
                var fileSize = fileInfo?.FileSize;
                
                _logger.LogInformation("Serving file: {FileName}, ContentType: {ContentType}, Size: {Size}", 
                    fileName, contentType, fileSize ?? 0);

                // Set appropriate headers
                Response.Headers.Append("Content-Disposition", $"inline; filename=\"{fileName}\"");
                
                return File(stream, contentType, fileName, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving legacy media file: {FileName}", fileName);
                return StatusCode(500, new { message = "Error retrieving media file", error = ex.Message });
            }
        }

        /// <summary>
        /// Get media file URL (does not download, just returns the URL)
        /// GET /api/LegacyMedia/url/{fileName}
        /// </summary>
        [HttpGet("url/{fileName}")]
        public IActionResult GetMediaFileUrl(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "File name is required" });
            }

            var url = _legacyMediaService.GetMediaFileUrl(fileName);
            
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest(new { message = "Legacy media API is not configured" });
            }

            return Ok(new { fileName, url });
        }

        /// <summary>
        /// Check if media file exists
        /// GET /api/LegacyMedia/check/{fileName}
        /// </summary>
        [HttpGet("check/{fileName}")]
        public async Task<IActionResult> CheckMediaFileExists(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "File name is required" });
            }

            var exists = await _legacyMediaService.CheckMediaFileExistsAsync(fileName, cancellationToken);
            return Ok(new { fileName, exists });
        }

        /// <summary>
        /// Test connection to legacy media API and verify file access
        /// GET /api/LegacyMedia/test/{fileName}
        /// </summary>
        [HttpGet("test/{fileName}")]
        public async Task<IActionResult> TestMediaFileAccess(string fileName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "File name is required" });
            }

            try
            {
                var results = new
                {
                    fileName,
                    timestamp = DateTime.UtcNow,
                    tests = new List<object>()
                };

                var testResults = new List<object>();

                // Test 1: Check via API
                try
                {
                    var exists = await _legacyMediaService.CheckMediaFileExistsAsync(fileName, cancellationToken);
                    testResults.Add(new
                    {
                        method = "API Check",
                        success = exists,
                        message = exists ? "File found via API" : "File not found via API"
                    });
                }
                catch (Exception ex)
                {
                    testResults.Add(new
                    {
                        method = "API Check",
                        success = false,
                        message = $"Error: {ex.Message}",
                        error = ex.GetType().Name
                    });
                }

                // Test 2: Try to get file info
                try
                {
                    var fileInfo = await _legacyMediaService.GetMediaFileInfoAsync(fileName, cancellationToken);
                    testResults.Add(new
                    {
                        method = "Get File Info",
                        success = fileInfo != null && fileInfo.Exists,
                        message = fileInfo != null && fileInfo.Exists 
                            ? $"File exists. Size: {fileInfo.FileSize} bytes, Type: {fileInfo.ContentType}"
                            : "File info not available",
                        fileInfo = fileInfo != null ? new
                        {
                            exists = fileInfo.Exists,
                            fileSize = fileInfo.FileSize,
                            contentType = fileInfo.ContentType,
                            fileUrl = fileInfo.FileUrl
                        } : null
                    });
                }
                catch (Exception ex)
                {
                    testResults.Add(new
                    {
                        method = "Get File Info",
                        success = false,
                        message = $"Error: {ex.Message}",
                        error = ex.GetType().Name
                    });
                }

                // Test 3: Try to get file stream
                try
                {
                    var stream = await _legacyMediaService.GetMediaFileStreamAsync(fileName, cancellationToken);
                    testResults.Add(new
                    {
                        method = "Get File Stream",
                        success = stream != null,
                        message = stream != null 
                            ? "File stream retrieved successfully" 
                            : "Could not retrieve file stream",
                        canRead = stream?.CanRead ?? false
                    });
                    
                    stream?.Dispose();
                }
                catch (Exception ex)
                {
                    testResults.Add(new
                    {
                        method = "Get File Stream",
                        success = false,
                        message = $"Error: {ex.Message}",
                        error = ex.GetType().Name
                    });
                }

                return Ok(new
                {
                    success = true,
                    fileName,
                    timestamp = DateTime.UtcNow,
                    tests = testResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing media file access: {FileName}", fileName);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error testing media file access",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Verify connection to legacy media API
        /// GET /api/LegacyMedia/verify
        /// </summary>
        [HttpGet("verify")]
        public async Task<IActionResult> VerifyConnection()
        {
            try
            {
                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var legacyApiBaseUrl = config["ConnectionSettings:LegacyMediaApiBaseUrl"];
                var legacyMediaPaths = config["ConnectionSettings:LegacyMediaPaths"];

                var verification = new
                {
                    timestamp = DateTime.UtcNow,
                    configuration = new
                    {
                        legacyApiBaseUrl,
                        legacyMediaPaths = legacyMediaPaths?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim())
                            .ToArray() ?? Array.Empty<string>(),
                        configured = !string.IsNullOrEmpty(legacyApiBaseUrl)
                    },
                    tests = new List<object>()
                };

                // Test API connection
                if (!string.IsNullOrEmpty(legacyApiBaseUrl))
                {
                    try
                    {
                        var httpClientFactory = HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var httpClient = httpClientFactory.CreateClient("LegacyMediaApi");
                        
                        // Try to ping the API (use a common endpoint or HEAD request)
                        var testUrl = "/api/Media/files/test.jpg"; // This will likely 404, but we can check if server responds
                        var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, testUrl));
                        
                        verification.tests.Add(new
                        {
                            test = "API Connection",
                            success = true,
                            message = $"API responded with status: {response.StatusCode}",
                            baseAddress = httpClient.BaseAddress?.ToString(),
                            statusCode = response.StatusCode.ToString()
                        });
                    }
                    catch (Exception ex)
                    {
                        verification.tests.Add(new
                        {
                            test = "API Connection",
                            success = false,
                            message = $"Failed to connect: {ex.Message}",
                            error = ex.GetType().Name
                        });
                    }
                }
                else
                {
                    verification.tests.Add(new
                    {
                        test = "API Connection",
                        success = false,
                        message = "LegacyMediaApiBaseUrl not configured"
                    });
                }

                return Ok(new
                {
                    success = true,
                    verification
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying legacy media connection");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error verifying connection",
                    error = ex.Message
                });
                }
        }
    }
}


