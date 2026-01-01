using Microsoft.Extensions.Options;
using SoitMed.Common;
using System.Net.Http.Headers;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for accessing legacy media files through the legacy media API
    /// </summary>
    public class LegacyMediaService : ILegacyMediaService
    {
        private readonly ConnectionSettings _connectionSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LegacyMediaService> _logger;

        public LegacyMediaService(
            IOptions<ConnectionSettings> connectionSettings,
            IHttpClientFactory httpClientFactory,
            ILogger<LegacyMediaService> logger)
        {
            _connectionSettings = connectionSettings.Value;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("LegacyMediaApi");
            
            // Set base URL if configured
            if (!string.IsNullOrEmpty(_connectionSettings.LegacyMediaApiBaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_connectionSettings.LegacyMediaApiBaseUrl);
            }
        }

        public string GetMediaFileUrl(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            return _connectionSettings.GetLegacyMediaFileUrl(fileName);
        }

        public async Task<bool> CheckMediaFileExistsAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(_connectionSettings.LegacyMediaApiBaseUrl))
                return false;

            try
            {
                var url = GetMediaFileUrl(fileName);
                var response = await _httpClient.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, url),
                    cancellationToken);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking media file existence: {FileName}", fileName);
                return false;
            }
        }

        public async Task<Stream?> GetMediaFileStreamAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(_connectionSettings.LegacyMediaApiBaseUrl))
                return null;

            try
            {
                var url = GetMediaFileUrl(fileName);
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStreamAsync(cancellationToken);
                }

                _logger.LogWarning("Media file not found: {FileName}, Status: {Status}", fileName, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving media file stream: {FileName}", fileName);
                return null;
            }
        }

        public async Task<LegacyMediaFileInfo?> GetMediaFileInfoAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(_connectionSettings.LegacyMediaApiBaseUrl))
                return null;

            try
            {
                var url = GetMediaFileUrl(fileName);
                var response = await _httpClient.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, url),
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    var contentLength = response.Content.Headers.ContentLength;
                    var lastModified = response.Content.Headers.LastModified?.DateTime;

                    return new LegacyMediaFileInfo
                    {
                        FileName = fileName,
                        FileUrl = url,
                        FileSize = contentLength,
                        ContentType = contentType,
                        Exists = true,
                        LastModified = lastModified
                    };
                }

                return new LegacyMediaFileInfo
                {
                    FileName = fileName,
                    FileUrl = url,
                    Exists = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting media file info: {FileName}", fileName);
                return new LegacyMediaFileInfo
                {
                    FileName = fileName,
                    FileUrl = GetMediaFileUrl(fileName),
                    Exists = false
                };
            }
        }
    }
}

