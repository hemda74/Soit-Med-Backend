using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SoitMed.Services
{
    public class PdfUploadService : IPdfUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".pdf" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public PdfUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<PdfUploadResult> UploadPdfAsync(IFormFile pdfFile, string folderPath)
        {
            try
            {
                // Validate file
                if (!IsValidPdfFile(pdfFile))
                {
                    return new PdfUploadResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid PDF file. Please upload a valid PDF file (max 5MB)."
                    };
                }

                // Create directory if it doesn't exist
                var uploadPath = Path.Combine(_environment.WebRootPath, folderPath);
                Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(pdfFile.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await pdfFile.CopyToAsync(stream);
                }

                // Return relative path for database storage
                var relativePath = Path.Combine(folderPath, fileName).Replace("\\", "/");

                return new PdfUploadResult
                {
                    Success = true,
                    FileName = pdfFile.FileName,
                    FilePath = relativePath,
                    ContentType = pdfFile.ContentType,
                    FileSize = pdfFile.Length
                };
            }
            catch (Exception ex)
            {
                return new PdfUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Error uploading PDF: {ex.Message}"
                };
            }
        }

        public Task<bool> DeletePdfAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return Task.FromResult(false);

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

        public bool IsValidPdfFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedExtensions.Contains(fileExtension) || 
                   file.ContentType == "application/pdf";
        }
    }
}

