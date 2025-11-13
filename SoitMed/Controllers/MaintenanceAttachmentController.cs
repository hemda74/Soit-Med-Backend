using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaintenanceAttachmentController : BaseController
    {
        private readonly IMaintenanceAttachmentService _attachmentService;
        private readonly ILogger<MaintenanceAttachmentController> _logger;

        public MaintenanceAttachmentController(
            IMaintenanceAttachmentService attachmentService,
            UserManager<ApplicationUser> userManager,
            ILogger<MaintenanceAttachmentController> logger)
            : base(userManager)
        {
            _attachmentService = attachmentService;
            _logger = logger;
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> UploadAttachment(
            [FromForm] IFormFile file,
            [FromForm] int maintenanceRequestId,
            [FromForm] AttachmentType attachmentType,
            [FromForm] string? description = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File is required");

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var attachment = await _attachmentService.UploadAttachmentAsync(
                    file,
                    maintenanceRequestId,
                    attachmentType,
                    description,
                    userId);

                return SuccessResponse(new
                {
                    Id = attachment.Id,
                    FileName = attachment.FileName,
                    FilePath = attachment.FilePath,
                    FileType = attachment.FileType,
                    FileSize = attachment.FileSize,
                    AttachmentType = attachment.AttachmentType,
                    UploadedAt = attachment.UploadedAt
                }, "Attachment uploaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Doctor,Technician,Manager,MaintenanceSupport")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            try
            {
                var result = await _attachmentService.DeleteAttachmentAsync(id);
                if (!result)
                    return NotFound("Attachment not found");

                return SuccessResponse(null, "Attachment deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttachment(int id)
        {
            try
            {
                var attachment = await _attachmentService.GetAttachmentAsync(id);
                if (attachment == null)
                    return NotFound();

                return SuccessResponse(new
                {
                    Id = attachment.Id,
                    MaintenanceRequestId = attachment.MaintenanceRequestId,
                    FileName = attachment.FileName,
                    FilePath = attachment.FilePath,
                    FileType = attachment.FileType,
                    FileSize = attachment.FileSize,
                    AttachmentType = attachment.AttachmentType,
                    Description = attachment.Description,
                    UploadedAt = attachment.UploadedAt,
                    UploadedBy = attachment.UploadedBy?.UserName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachment {AttachmentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("request/{maintenanceRequestId}")]
        public async Task<IActionResult> GetAttachmentsByRequest(int maintenanceRequestId)
        {
            try
            {
                var attachments = await _attachmentService.GetAttachmentsByRequestAsync(maintenanceRequestId);
                var result = attachments.Select(a => new
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    AttachmentType = a.AttachmentType,
                    Description = a.Description,
                    UploadedAt = a.UploadedAt,
                    UploadedBy = a.UploadedBy?.UserName
                });

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachments for request {RequestId}", maintenanceRequestId);
                return ErrorResponse(ex.Message);
            }
        }
    }
}

