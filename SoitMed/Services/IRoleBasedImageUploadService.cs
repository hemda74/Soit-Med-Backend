using Microsoft.AspNetCore.Http;
using SoitMed.Models.Identity;
using System.IO;

namespace SoitMed.Services
{
    public interface IRoleBasedImageUploadService
    {
        Task<ImageUploadResult> UploadUserImageAsync(
            IFormFile imageFile, 
            ApplicationUser user, 
            string role, 
            string? departmentName = null, 
            string? altText = null);
        
        Task<bool> DeleteUserImageAsync(string filePath);
        bool IsValidImageFile(IFormFile file);
        string GenerateUserFolderName(ApplicationUser user, string? departmentName = null);
    }

    public class RoleBasedImageUploadService : IRoleBasedImageUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadsRootPhysicalPath; // physical folder in wwwroot for project-based storage
        private readonly ILogger<RoleBasedImageUploadService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public RoleBasedImageUploadService(IWebHostEnvironment environment, ILogger<RoleBasedImageUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
            // Always use wwwroot/uploads for project-based storage
            var defaultUploadsRoot = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(defaultUploadsRoot);
            _uploadsRootPhysicalPath = defaultUploadsRoot;
        }

        public async Task<ImageUploadResult> UploadUserImageAsync(
            IFormFile imageFile, 
            ApplicationUser user, 
            string role, 
            string? departmentName = null, 
            string? altText = null)
        {
            _logger.LogInformation("UploadUserImageAsync called for user {UserId}", user.Id);
            string? filePath = null;
            try
            {
                // Validate file
                if (!IsValidImageFile(imageFile))
                {
                    _logger.LogWarning("Invalid image file for user {UserId}", user.Id);
                    return new ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid image file. Please upload a valid JPG, JPEG, PNG, or GIF image (max 5MB)."
                    };
                }

                // Generate folder structure: role/firstname_lastname_departmentname_userid
                var userFolderName = GenerateUserFolderName(user, departmentName);
                var roleFolder = role.ToLowerInvariant();
                var relativeFolderPath = Path.Combine("uploads", roleFolder, userFolderName);
                _logger.LogInformation("FolderPath: {FolderPath}", relativeFolderPath);

                // Create physical directory outside the project to avoid hot reload restarts
                var uploadPath = Path.Combine(_uploadsRootPhysicalPath, roleFolder, userFolderName);
                _logger.LogInformation("UploadPath: {UploadPath}", uploadPath);
                Directory.CreateDirectory(uploadPath);
                _logger.LogInformation("Directory created successfully");

                // Generate unique filename
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                filePath = Path.Combine(uploadPath, fileName);
                _logger.LogInformation("FilePath: {FilePath}", filePath);

                // Save file with proper stream handling and memory management
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true))
                {
                    // Use a larger buffer for better performance
                    var buffer = new byte[8192];
                    using (var inputStream = imageFile.OpenReadStream())
                    {
                        int bytesRead;
                        while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await stream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }
                    await stream.FlushAsync();
                }
                _logger.LogInformation("File saved successfully");

                // Return relative path for database storage
                var relativePath = Path.Combine(relativeFolderPath, fileName).Replace("\\", "/");

                return new ImageUploadResult
                {
                    Success = true,
                    FileName = fileName,
                    FilePath = relativePath,
                    ContentType = imageFile.ContentType,
                    FileSize = imageFile.Length,
                    AltText = altText
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for user {UserId}", user.Id);
                
                // Clean up partially created file
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        _logger.LogInformation("Cleaned up partially created file: {FilePath}", filePath);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Failed to clean up file: {FilePath}", filePath);
                    }
                }
                
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Error uploading image: {ex.Message}"
                };
            }
        }

        public Task<bool> DeleteUserImageAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedExtensions.Contains(fileExtension);
        }

        public string GenerateUserFolderName(ApplicationUser user, string? departmentName = null)
        {
            var firstName = user.FirstName ?? "Unknown";
            var lastName = user.LastName ?? "User";
            var deptName = departmentName ?? "NoDepartment";
            var userId = user.Id;

            // Clean names for folder compatibility
            firstName = CleanFolderName(firstName);
            lastName = CleanFolderName(lastName);
            deptName = CleanFolderName(deptName);

            return $"{firstName}_{lastName}_{deptName}_{userId}";
        }

        private string CleanFolderName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Unknown";

            // Remove invalid characters for folder names
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
            
            // Replace spaces with underscores
            cleaned = cleaned.Replace(" ", "_");
            
            // Limit length
            if (cleaned.Length > 50)
                cleaned = cleaned.Substring(0, 50);

            return string.IsNullOrEmpty(cleaned) ? "Unknown" : cleaned;
        }
    }
}
