using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SoitMed.Services
{
    public class VoiceUploadService : IVoiceUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".mp3", ".wav", ".m4a", ".aac", ".ogg" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB for voice files

        public VoiceUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<VoiceUploadResult> UploadVoiceFileAsync(IFormFile voiceFile, string userId, string? folderPath = null)
        {
            try
            {
                // Validate file
                if (!IsValidVoiceFile(voiceFile))
                {
                    return new VoiceUploadResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid voice file. Please upload a valid MP3, WAV, M4A, AAC, or OGG file (max 10MB)."
                    };
                }

                // Use default folder if not specified
                var uploadFolder = folderPath ?? "uploads/voice-descriptions";
                var userFolder = Path.Combine(uploadFolder, userId);
                
                // Create directory if it doesn't exist
                var uploadPath = Path.Combine(_environment.WebRootPath, userFolder);
                Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(voiceFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension))
                {
                    fileExtension = ".m4a"; // Default extension for voice recordings
                }
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await voiceFile.CopyToAsync(stream);
                }

                // Return relative path for database storage
                var relativePath = Path.Combine(userFolder, fileName).Replace("\\", "/");

                return new VoiceUploadResult
                {
                    Success = true,
                    FileName = voiceFile.FileName,
                    FilePath = relativePath,
                    ContentType = voiceFile.ContentType ?? "audio/mpeg",
                    FileSize = voiceFile.Length
                };
            }
            catch (Exception ex)
            {
                return new VoiceUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Error uploading voice file: {ex.Message}"
                };
            }
        }

        public Task<bool> DeleteVoiceFileAsync(string filePath)
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

        public bool IsValidVoiceFile(IFormFile file)
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

