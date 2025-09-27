using Microsoft.AspNetCore.Http;

namespace SoitMed.Services
{
    public interface IImageUploadService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile imageFile, string folderPath, string? altText = null);
        Task<bool> DeleteImageAsync(string filePath);
        bool IsValidImageFile(IFormFile file);
    }

    public class ImageUploadResult
    {
        public bool Success { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public string? AltText { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

