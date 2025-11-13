using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SoitMed.Services
{
    public interface IOfferEquipmentImageService
    {
        Task<ImageUploadResult> UploadEquipmentImageAsync(IFormFile imageFile, long offerId, long equipmentId, string? altText = null);
        Task<bool> DeleteEquipmentImageAsync(string filePath);
        bool IsValidImageFile(IFormFile file);
    }

    public class OfferEquipmentImageService : IOfferEquipmentImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsRootPhysicalPath;
        private readonly ILogger<OfferEquipmentImageService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public OfferEquipmentImageService(IWebHostEnvironment environment, ILogger<OfferEquipmentImageService> logger)
        {
            _environment = environment;
            _logger = logger;
            var defaultUploadsRoot = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(defaultUploadsRoot);
            _uploadsRootPhysicalPath = defaultUploadsRoot;
        }

        public async Task<ImageUploadResult> UploadEquipmentImageAsync(IFormFile imageFile, long offerId, long equipmentId, string? altText = null)
        {
            _logger.LogInformation("UploadEquipmentImageAsync called for offer {OfferId}, equipment {EquipmentId}", offerId, equipmentId);
            
            string? filePath = null;
            try
            {
                // Validate file
                if (!IsValidImageFile(imageFile))
                {
                    _logger.LogWarning("Invalid image file for offer {OfferId}, equipment {EquipmentId}", offerId, equipmentId);
                    return new ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid image file. Please upload a valid JPG, JPEG, PNG, or GIF image (max 5MB)."
                    };
                }

                // Generate folder structure: offers/{offerId}
                var relativeFolderPath = Path.Combine("offers", offerId.ToString());
                _logger.LogInformation("FolderPath: {FolderPath}", relativeFolderPath);

                // Create physical directory
                var uploadPath = Path.Combine(_environment.WebRootPath, "offers", offerId.ToString());
                _logger.LogInformation("UploadPath: {UploadPath}", uploadPath);
                Directory.CreateDirectory(uploadPath);
                _logger.LogInformation("Directory created successfully");

                // Generate unique filename with equipment ID prefix
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var fileName = $"equipment-{equipmentId}-{Guid.NewGuid()}{fileExtension}";
                filePath = Path.Combine(uploadPath, fileName);
                _logger.LogInformation("FilePath: {FilePath}", filePath);

                // Save file with proper stream handling and memory management
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Return relative path for database storage: offers/{offerId}/equipment-{equipmentId}-{guid}.jpg
                var relativePath = Path.Combine("offers", offerId.ToString(), fileName).Replace("\\", "/");

                _logger.LogInformation("Image uploaded successfully. RelativePath: {RelativePath}", relativePath);

                return new ImageUploadResult
                {
                    Success = true,
                    FilePath = relativePath,
                    FileName = fileName,
                    FileSize = imageFile.Length,
                    ContentType = imageFile.ContentType,
                    AltText = altText
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading equipment image for offer {OfferId}, equipment {EquipmentId}", offerId, equipmentId);
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while uploading the image: {ex.Message}"
                };
            }
        }

        public Task<bool> DeleteEquipmentImageAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Equipment image deleted successfully: {FilePath}", filePath);
                    return Task.FromResult(true);
                }
                _logger.LogWarning("Equipment image file not found: {FilePath}", filePath);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment image: {FilePath}", filePath);
                return Task.FromResult(false);
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            if (file.Length > _maxFileSize)
                return false;

            // Validate content type
            var contentType = file.ContentType;
            if (!contentType.StartsWith("image/"))
                return false;

            return true;
        }
    }
}

