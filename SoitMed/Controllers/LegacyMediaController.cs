using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Services;

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
                    return BadRequest(new { message = "Invalid file name" });
                }

                var fileInfo = await _legacyMediaService.GetMediaFileInfoAsync(fileName, cancellationToken);
                
                if (fileInfo == null || !fileInfo.Exists)
                {
                    return NotFound(new { message = $"File '{fileName}' not found" });
                }

                var stream = await _legacyMediaService.GetMediaFileStreamAsync(fileName, cancellationToken);
                if (stream == null)
                {
                    return NotFound(new { message = $"File '{fileName}' could not be retrieved" });
                }

                var contentType = fileInfo.ContentType ?? "application/octet-stream";
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
    }
}

