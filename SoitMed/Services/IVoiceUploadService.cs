namespace SoitMed.Services
{
    public interface IVoiceUploadService
    {
        Task<VoiceUploadResult> UploadVoiceFileAsync(IFormFile voiceFile, string userId, string? folderPath = null);
        Task<bool> DeleteVoiceFileAsync(string filePath);
        bool IsValidVoiceFile(IFormFile file);
    }

    public class VoiceUploadResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
    }
}

