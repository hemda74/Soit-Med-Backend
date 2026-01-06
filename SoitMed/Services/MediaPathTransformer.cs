using System.Text.RegularExpressions;

namespace SoitMed.Services
{
    /// <summary>
    /// Transforms legacy media file paths to new API URL format
    /// Based on logic from soitmed_data_backend
    /// </summary>
    public class MediaPathTransformer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MediaPathTransformer> _logger;
        private readonly string? _legacyMediaApiBaseUrl;

        // Legacy file path patterns to detect and transform
        private readonly string[] _legacyPathPatterns = new[]
        {
            @"D:\\Soit-Med\\legacy\\SOIT\\UploadFiles\\Files",
            @"D:\\Soit-Med\\legacy\\SOIT\\UploadFiles\\Images",
            @"D:\\Soit-Med\\legacy\\SOIT\\Ar\\MNT\\FileUploaders\\Reports",
            @"C:\\Soit-Med\\legacy\\SOIT\\UploadFiles\\Files",
            @"C:\\Soit-Med\\legacy\\SOIT\\UploadFiles\\Images",
            @"C:\\Soit-Med\\legacy\\SOIT\\Ar\\MNT\\FileUploaders\\Reports"
        };

        public MediaPathTransformer(IConfiguration configuration, ILogger<MediaPathTransformer> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _legacyMediaApiBaseUrl = _configuration["ConnectionSettings:LegacyMediaApiBaseUrl"];
        }

        /// <summary>
        /// Transforms a legacy file path to new API URL format
        /// Example: "D:\Soit-Med\legacy\SOIT\UploadFiles\Files\image.jpg" -> "/api/LegacyMedia/files/image.jpg"
        /// </summary>
        public string TransformPath(string? legacyPath)
        {
            if (string.IsNullOrWhiteSpace(legacyPath))
                return string.Empty;

            try
            {
                // Extract filename from path
                string fileName = ExtractFileName(legacyPath);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    _logger.LogWarning("Could not extract filename from legacy path: {LegacyPath}", legacyPath);
                    return string.Empty;
                }

                // Build new API URL
                if (!string.IsNullOrEmpty(_legacyMediaApiBaseUrl))
                {
                    // Use full URL if base URL is configured
                    return $"{_legacyMediaApiBaseUrl}/api/Media/files/{Uri.EscapeDataString(fileName)}";
                }
                else
                {
                    // Use relative path if no base URL configured
                    return $"/api/LegacyMedia/files/{Uri.EscapeDataString(fileName)}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming legacy path: {LegacyPath}", legacyPath);
                return string.Empty;
            }
        }

        /// <summary>
        /// Transforms multiple file paths (pipe/comma/semicolon separated)
        /// Example: "file1.jpg|file2.jpg" -> "/api/LegacyMedia/files/file1.jpg,/api/LegacyMedia/files/file2.jpg"
        /// </summary>
        public string TransformMultiplePaths(string? legacyPaths)
        {
            if (string.IsNullOrWhiteSpace(legacyPaths))
                return string.Empty;

            try
            {
                // Parse pipe/comma/semicolon separated file names
                var fileNames = legacyPaths
                    .Split(new[] { '|', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(f => f.Trim())
                    .Where(f => !string.IsNullOrWhiteSpace(f))
                    .Distinct()
                    .ToList();

                var transformedPaths = fileNames
                    .Select(fileName => TransformPath(fileName))
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .ToList();

                return string.Join(",", transformedPaths);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming multiple legacy paths: {LegacyPaths}", legacyPaths);
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts filename from legacy path
        /// Handles both full paths and just filenames
        /// </summary>
        private string ExtractFileName(string path)
        {
            // If it's already just a filename (no path separators), return as-is
            if (!path.Contains('\\') && !path.Contains('/'))
            {
                return path.Trim();
            }

            // Remove legacy path prefixes
            string cleanedPath = path;
            foreach (var pattern in _legacyPathPatterns)
            {
                if (cleanedPath.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    cleanedPath = cleanedPath.Substring(pattern.Length).TrimStart('\\', '/');
                    break;
                }
            }

            // Extract filename from path
            var fileName = Path.GetFileName(cleanedPath);
            
            // If still contains path separators, get the last part
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains('\\') || fileName.Contains('/'))
            {
                var parts = cleanedPath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                fileName = parts.LastOrDefault() ?? cleanedPath;
            }

            return fileName.Trim();
        }

        /// <summary>
        /// Checks if a path is a legacy path that needs transformation
        /// </summary>
        public bool IsLegacyPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return _legacyPathPatterns.Any(pattern => 
                path.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(pattern, StringComparison.OrdinalIgnoreCase));
        }
    }
}

