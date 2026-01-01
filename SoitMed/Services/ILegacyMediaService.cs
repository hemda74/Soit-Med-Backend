namespace SoitMed.Services
{
    /// <summary>
    /// Service for accessing legacy media files through the legacy media API
    /// </summary>
    public interface ILegacyMediaService
    {
        /// <summary>
        /// Get media file URL for a legacy file
        /// </summary>
        string GetMediaFileUrl(string fileName);

        /// <summary>
        /// Check if a media file exists
        /// </summary>
        Task<bool> CheckMediaFileExistsAsync(string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get media file as stream (proxy from legacy API)
        /// </summary>
        Task<Stream?> GetMediaFileStreamAsync(string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get media file info (size, type, etc.)
        /// </summary>
        Task<LegacyMediaFileInfo?> GetMediaFileInfoAsync(string fileName, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Information about a legacy media file
    /// </summary>
    public class LegacyMediaFileInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long? FileSize { get; set; }
        public string? ContentType { get; set; }
        public bool Exists { get; set; }
        public DateTime? LastModified { get; set; }
    }
}

