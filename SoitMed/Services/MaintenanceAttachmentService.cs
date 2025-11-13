using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;
using SoitMed.Repositories;
using System.IO;

namespace SoitMed.Services
{
    public class MaintenanceAttachmentService : IMaintenanceAttachmentService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MaintenanceAttachmentService> _logger;

        // Allowed extensions for each attachment type
        private readonly Dictionary<AttachmentType, string[]> _allowedExtensions = new()
        {
            { AttachmentType.Image, new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" } },
            { AttachmentType.Video, new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm" } },
            { AttachmentType.Audio, new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac", ".wma" } },
            { AttachmentType.Document, new[] { ".pdf", ".doc", ".docx", ".txt", ".xls", ".xlsx" } },
            { AttachmentType.Other, new[] { ".*" } }
        };

        // Max file sizes (in bytes)
        private readonly Dictionary<AttachmentType, long> _maxFileSizes = new()
        {
            { AttachmentType.Image, 10 * 1024 * 1024 },      // 10MB
            { AttachmentType.Video, 100 * 1024 * 1024 },      // 100MB
            { AttachmentType.Audio, 20 * 1024 * 1024 },       // 20MB
            { AttachmentType.Document, 10 * 1024 * 1024 },    // 10MB
            { AttachmentType.Other, 50 * 1024 * 1024 }       // 50MB
        };

        public MaintenanceAttachmentService(
            IWebHostEnvironment environment,
            IUnitOfWork unitOfWork,
            ILogger<MaintenanceAttachmentService> logger)
        {
            _environment = environment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<MaintenanceRequestAttachment> UploadAttachmentAsync(
            IFormFile file,
            int maintenanceRequestId,
            AttachmentType attachmentType,
            string? description = null,
            string? uploadedById = null)
        {
            try
            {
                // Validate file
                if (!IsValidFile(file, attachmentType))
                {
                    var allowedExts = string.Join(", ", _allowedExtensions[attachmentType]);
                    var maxSizeMB = _maxFileSizes[attachmentType] / (1024 * 1024);
                    throw new ArgumentException(
                        $"Invalid file. Allowed extensions: {allowedExts}, Max size: {maxSizeMB}MB");
                }

                // Verify maintenance request exists
                var maintenanceRequest = await _unitOfWork.MaintenanceRequests.GetByIdAsync(maintenanceRequestId);
                if (maintenanceRequest == null)
                    throw new ArgumentException("Maintenance request not found", nameof(maintenanceRequestId));

                // Create folder structure: maintenance-requests/{requestId}/attachments
                var folderPath = Path.Combine("maintenance-requests", maintenanceRequestId.ToString(), "attachments");
                var uploadPath = Path.Combine(_environment.WebRootPath, folderPath);
                Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path for database storage
                var relativePath = Path.Combine(folderPath, fileName).Replace("\\", "/");

                // Create attachment record
                var attachment = new MaintenanceRequestAttachment
                {
                    MaintenanceRequestId = maintenanceRequestId,
                    FilePath = relativePath,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    AttachmentType = attachmentType,
                    Description = description,
                    UploadedById = uploadedById,
                    UploadedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.MaintenanceRequestAttachments.CreateAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Attachment uploaded. AttachmentId: {AttachmentId}, RequestId: {RequestId}, Type: {Type}",
                    attachment.Id, maintenanceRequestId, attachmentType);

                return attachment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment for request {RequestId}", maintenanceRequestId);
                throw;
            }
        }

        public async Task<bool> DeleteAttachmentAsync(int attachmentId)
        {
            try
            {
                var attachment = await _unitOfWork.MaintenanceRequestAttachments.GetByIdAsync(attachmentId);
                if (attachment == null)
                    return false;

                // Delete physical file
                var fullPath = Path.Combine(_environment.WebRootPath, attachment.FilePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                // Soft delete - mark as inactive
                attachment.IsActive = false;
                await _unitOfWork.MaintenanceRequestAttachments.UpdateAsync(attachment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Attachment deleted. AttachmentId: {AttachmentId}", attachmentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
                return false;
            }
        }

        public async Task<MaintenanceRequestAttachment?> GetAttachmentAsync(int attachmentId)
        {
            return await _unitOfWork.MaintenanceRequestAttachments.GetByIdAsync(attachmentId);
        }

        public async Task<IEnumerable<MaintenanceRequestAttachment>> GetAttachmentsByRequestAsync(int maintenanceRequestId)
        {
            return await _unitOfWork.MaintenanceRequestAttachments
                .GetByMaintenanceRequestIdAsync(maintenanceRequestId);
        }

        public bool IsValidFile(IFormFile file, AttachmentType attachmentType)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file size
            if (file.Length > _maxFileSizes[attachmentType])
                return false;

            // Check file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExts = _allowedExtensions[attachmentType];

            // For "Other" type, allow all extensions
            if (attachmentType == AttachmentType.Other)
                return true;

            return allowedExts.Contains(fileExtension);
        }
    }
}

