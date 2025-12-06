using Microsoft.AspNetCore.Http;

namespace SoitMed.Services
{
    public interface IPdfUploadService
    {
        Task<PdfUploadResult> UploadPdfAsync(IFormFile pdfFile, string folderPath);
        Task<bool> DeletePdfAsync(string filePath);
        bool IsValidPdfFile(IFormFile file);
    }

    public class PdfUploadResult
    {
        public bool Success { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

