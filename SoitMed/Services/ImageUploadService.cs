using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SoitMed.Services
{
    public class ImageUploadService : IImageUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile imageFile, string folderPath, string? altText = null)
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

                // Create directory if it doesn't exist
                var uploadPath = Path.Combine(_environment.WebRootPath, folderPath);
                Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
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

        public Task<bool> DeleteImageAsync(string filePath)
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
    }
}

