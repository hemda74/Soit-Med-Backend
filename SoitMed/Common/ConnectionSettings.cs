namespace SoitMed.Common
{
    /// <summary>
    /// Connection settings for database and media file access
    /// Supports both Local and Remote modes
    /// </summary>
    public class ConnectionSettings
    {
        public const string SectionName = "ConnectionSettings";

        /// <summary>
        /// Connection mode: "Local" or "Remote"
        /// </summary>
        public string Mode { get; set; } = "Local";

        /// <summary>
        /// Connection string for local database (localhost)
        /// </summary>
        public string LocalConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Connection string for remote database (IP 104)
        /// </summary>
        public string RemoteConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Local media files path (when working on same machine as DB)
        /// </summary>
        public string LocalMediaPath { get; set; } = string.Empty;

        /// <summary>
        /// Remote media files path (network share or UNC path)
        /// Format: \\10.10.9.100\LegacyMedia or mapped drive
        /// </summary>
        public string RemoteMediaPath { get; set; } = string.Empty;

        /// <summary>
        /// Legacy Media API Base URL (for serving legacy media files)
        /// Example: http://localhost:5266 or http://10.10.9.104:5266
        /// </summary>
        public string LegacyMediaApiBaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Legacy media file paths (comma-separated or JSON array)
        /// Paths where legacy media files are stored
        /// </summary>
        public string LegacyMediaPaths { get; set; } = string.Empty;

        /// <summary>
        /// Gets the active connection string based on Mode
        /// </summary>
        public string GetActiveConnectionString()
        {
            return Mode.Equals("Remote", StringComparison.OrdinalIgnoreCase)
                ? RemoteConnectionString
                : LocalConnectionString;
        }

        /// <summary>
        /// Gets the active media path based on Mode
        /// </summary>
        public string GetActiveMediaPath()
        {
            return Mode.Equals("Remote", StringComparison.OrdinalIgnoreCase)
                ? RemoteMediaPath
                : LocalMediaPath;
        }

        /// <summary>
        /// Gets legacy media file URL for a given filename
        /// </summary>
        public string GetLegacyMediaFileUrl(string fileName)
        {
            if (string.IsNullOrEmpty(LegacyMediaApiBaseUrl))
                return string.Empty;

            var baseUrl = LegacyMediaApiBaseUrl.TrimEnd('/');
            var encodedFileName = Uri.EscapeDataString(fileName);
            return $"{baseUrl}/api/Media/files/{encodedFileName}";
        }

        /// <summary>
        /// Gets list of legacy media paths as array
        /// </summary>
        public List<string> GetLegacyMediaPathsList()
        {
            if (string.IsNullOrEmpty(LegacyMediaPaths))
                return new List<string>();

            try
            {
                // Try parsing as JSON array first
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(LegacyMediaPaths) 
                    ?? new List<string>();
            }
            catch
            {
                // Fallback to comma-separated
                return LegacyMediaPaths
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToList();
            }
        }

        /// <summary>
        /// Checks if current mode is Remote
        /// </summary>
        public bool IsRemoteMode()
        {
            return Mode.Equals("Remote", StringComparison.OrdinalIgnoreCase);
        }
    }
}

