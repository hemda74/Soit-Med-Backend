using Microsoft.AspNetCore.Http;
using SoitMed.Models.Identity;

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
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public RoleBasedImageUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<ImageUploadResult> UploadUserImageAsync(
            IFormFile imageFile, 
            ApplicationUser user, 
            string role, 
            string? departmentName = null, 
            string? altText = null)
        {
            try
            {
                // Validate file
                if (!IsValidImageFile(imageFile))
                {
                    return new ImageUploadResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid image file. Please upload a valid JPG, JPEG, PNG, or GIF image (max 5MB)."
                    };
                }

                // Generate folder structure: role/firstname_lastname_departmentname_userid
                var userFolderName = GenerateUserFolderName(user, departmentName);
                var roleFolder = role.ToLowerInvariant();
                var folderPath = Path.Combine("uploads", roleFolder, userFolderName);

                // Create directory if it doesn't exist
                var uploadPath = Path.Combine(_environment.WebRootPath, folderPath);
                Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var fileName = $"profile{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Return relative path for database storage
                var relativePath = Path.Combine(folderPath, fileName).Replace("\\", "/");

                return new ImageUploadResult
                {
                    Success = true,
                    FileName = imageFile.FileName,
                    FilePath = relativePath,
                    ContentType = imageFile.ContentType,
                    FileSize = imageFile.Length,
                    AltText = altText
                };
            }
            catch (Exception ex)
            {
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Error uploading image: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteUserImageAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
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
