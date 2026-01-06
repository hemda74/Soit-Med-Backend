using Microsoft.Extensions.Options;
using SoitMed.Common;
using System.Net.Http.Headers;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for accessing legacy media files directly from remote server (IP 104)
    /// Supports both direct file system access and API proxy
    /// </summary>
    public class LegacyMediaService : ILegacyMediaService
    {
        private readonly ConnectionSettings _connectionSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LegacyMediaService> _logger;
        private readonly string[] _legacyMediaPaths;
        private readonly string _remoteServerIp = "10.10.9.104";

        public LegacyMediaService(
            IOptions<ConnectionSettings> connectionSettings,
            IHttpClientFactory httpClientFactory,
            ILogger<LegacyMediaService> logger)
        {
            _connectionSettings = connectionSettings.Value;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("LegacyMediaApi");
            
            // BaseAddress should already be set in Program.cs, but ensure it's set
            if (_httpClient.BaseAddress == null && !string.IsNullOrEmpty(_connectionSettings.LegacyMediaApiBaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_connectionSettings.LegacyMediaApiBaseUrl.TrimEnd('/'));
                _logger.LogInformation("Set HttpClient BaseAddress to: {BaseAddress}", _httpClient.BaseAddress);
            }
            
            if (_httpClient.BaseAddress != null)
            {
                _logger.LogInformation("HttpClient BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
            }
            else
            {
                _logger.LogWarning("HttpClient BaseAddress is not configured. Legacy media API calls may fail.");
            }

            // Parse legacy media paths from configuration
            _legacyMediaPaths = string.IsNullOrEmpty(_connectionSettings.LegacyMediaPaths)
                ? new[] 
                { 
                    @"D:\Soit-Med\legacy\SOIT\UploadFiles\Files",
                    @"D:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports"
                }
                : _connectionSettings.LegacyMediaPaths.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToArray();
        }

        public string GetMediaFileUrl(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            // Return URL to our own API endpoint (not legacy API)
            // This allows frontend to use our API which will proxy to legacy API
            return $"/api/LegacyMedia/files/{Uri.EscapeDataString(fileName)}";
        }

        public async Task<bool> CheckMediaFileExistsAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            _logger.LogInformation("Checking if file exists: {FileName}", fileName);

            // Primary: Check via API (soitmed_data_backend) - This is the most reliable method
            // soitmed_data_backend runs on Windows and can access files directly
            // Our backend uses current database, soitmed_data_backend uses TBS (for file serving only)
            if (!string.IsNullOrEmpty(_connectionSettings.LegacyMediaApiBaseUrl) && _httpClient.BaseAddress != null)
            {
                try
                {
                    var apiUrl = $"/api/Media/files/{Uri.EscapeDataString(fileName)}";
                    // Set timeout to avoid long waits if API is down
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(10)); // 10 second timeout
                    
                    // Try HEAD first (lighter)
                    var request = new HttpRequestMessage(HttpMethod.Head, apiUrl);
                    var response = await _httpClient.SendAsync(request, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("File exists via API (HEAD): {FileName}", fileName);
                        return true;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
                    {
                        // HEAD not supported, try GET (but only check status, don't download)
                        _logger.LogDebug("HEAD not supported for {FileName}, trying GET...", fileName);
                        var getRequest = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                        var getResponse = await _httpClient.SendAsync(getRequest, cts.Token);
                        
                        if (getResponse.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("File exists via API (GET): {FileName}", fileName);
                            return true;
                        }
                        else
                        {
                            _logger.LogDebug("File not found via API: {FileName}, Status: {Status}", fileName, getResponse.StatusCode);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("File not found via API: {FileName}, Status: {Status}", fileName, response.StatusCode);
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("API request timeout for {FileName}. Trying direct file access as fallback.", fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking media file existence via API: {FileName}. Trying direct file access as fallback.", fileName);
                }
            }
            else
            {
                _logger.LogDebug("LegacyMediaApiBaseUrl not configured. Using direct file access only.");
            }

            // Fallback: Check direct file system access (works if running on same machine/network)
            // Note: UNC paths may not work on macOS/Linux
            var filePath = FindFileInLegacyPaths(fileName);
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                _logger.LogInformation("File exists via direct access: {FilePath}", filePath);
                return true;
            }

            _logger.LogWarning("File not found: {FileName}. Checked API and direct paths.", fileName);
            return false;
        }

        public async Task<Stream?> GetMediaFileStreamAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogWarning("GetMediaFileStreamAsync called with empty fileName");
                return null;
            }

            _logger.LogInformation("Attempting to retrieve media file: {FileName}", fileName);

            // Primary: Use API proxy (soitmed_data_backend handles file search in multiple paths and variations)
            // This is the most reliable method as soitmed_data_backend runs on the same machine as the files
            // soitmed_data_backend uses TBS database but serves files - our system uses current database
            if (!string.IsNullOrEmpty(_connectionSettings.LegacyMediaApiBaseUrl) && _httpClient.BaseAddress != null)
            {
                try
                {
                    // Use the legacy API endpoint directly (soitmed_data_backend uses /api/Media/files/)
                    // soitmed_data_backend will try multiple filename variations automatically
                    var apiUrl = $"/api/Media/files/{Uri.EscapeDataString(fileName)}";
                    var fullUrl = _httpClient.BaseAddress + apiUrl;
                    _logger.LogInformation("Requesting file from legacy API: {FullUrl}", fullUrl);
                    
                    // Set timeout to avoid long waits
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30 second timeout
                    
                    var response = await _httpClient.GetAsync(apiUrl, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Successfully retrieved file from API: {FileName}", fileName);
                        return await response.Content.ReadAsStreamAsync(cts.Token);
                    }

                    var errorResponse = await response.Content.ReadAsStringAsync(cts.Token);
                    _logger.LogWarning("Media file not found via API: {FileName}, Status: {Status}, Response: {Response}", 
                        fileName, response.StatusCode, errorResponse);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("API request timeout for {FileName}. soitmed_data_backend may be slow or unavailable.", fileName);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Cannot connect to soitmed_data_backend API at {BaseUrl}. Will try direct file access.", 
                        _connectionSettings.LegacyMediaApiBaseUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving media file stream via API: {FileName}", fileName);
                }
            }

            // Fallback: Try direct file system access (if running on same network/machine)
            // Note: UNC paths may not work on macOS/Linux, so API is preferred
            var filePath = FindFileInLegacyPaths(fileName);
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                try
                {
                    _logger.LogInformation("Serving file directly from path: {FilePath}", filePath);
                    return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read file directly from path: {FilePath}", filePath);
                }
            }
            else
            {
                _logger.LogWarning("File not found in any legacy paths: {FileName}. Consider using soitmed_data_backend API which handles file search better.", fileName);
            }

            return null;
        }

        /// <summary>
        /// Find file in legacy media paths
        /// Searches in multiple paths similar to soitmed_data_backend
        /// Tries multiple filename variations (case-insensitive, with/without prefixes)
        /// </summary>
        private string? FindFileInLegacyPaths(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            // Build filename variations (similar to soitmed_data_backend)
            var fileNameVariations = new List<string> { fileName };
            
            // Add variations: original, lowercase, uppercase
            fileNameVariations.Add(fileName.ToLower());
            fileNameVariations.Add(fileName.ToUpper());
            
            // Try with/without common prefixes
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var commonPrefixes = new[] { "Visit_", "VR_", "Report_", "Contract_", "Machine_", "IMG_", "IMG", "PIC_", "PIC", "_" };
            
            foreach (var prefix in commonPrefixes)
            {
                if (baseName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    var withoutPrefix = baseName.Substring(prefix.Length) + extension;
                    fileNameVariations.Add(withoutPrefix);
                    fileNameVariations.Add(withoutPrefix.ToLower());
                    fileNameVariations.Add(withoutPrefix.ToUpper());
                }
            }
            
            // Remove duplicates
            fileNameVariations = fileNameVariations.Distinct().ToList();

            // Build search paths (similar to soitmed_data_backend)
            var searchPaths = new List<string>();
            
            // Add configured paths
            foreach (var basePath in _legacyMediaPaths)
            {
                // Try local path first (if running on same machine)
                searchPaths.Add(basePath);
                
                // Try UNC path for remote access
                // Format depends on OS:
                // Windows: \\10.10.9.104\D$\...
                // macOS/Linux: smb://10.10.9.104/D$/... or //10.10.9.104/D$/...
                if (basePath.StartsWith(@"D:\"))
                {
                    // Windows UNC format
                    var uncPathWindows = basePath.Replace(@"D:\", $@"\\{_remoteServerIp}\D$\");
                    searchPaths.Add(uncPathWindows);
                    
                    // macOS/Linux SMB format (if SMB is mounted)
                    var uncPathUnix = basePath.Replace(@"D:\", $@"//{_remoteServerIp}/D$/");
                    searchPaths.Add(uncPathUnix);
                    
                    // Also try with forward slashes (some systems accept this)
                    var uncPathForward = basePath.Replace(@"D:\", $@"\\{_remoteServerIp}\D$\").Replace('\\', '/');
                    searchPaths.Add(uncPathForward);
                }
                else if (basePath.StartsWith(@"C:\"))
                {
                    // Windows UNC format
                    var uncPathWindows = basePath.Replace(@"C:\", $@"\\{_remoteServerIp}\C$\");
                    searchPaths.Add(uncPathWindows);
                    
                    // macOS/Linux SMB format
                    var uncPathUnix = basePath.Replace(@"C:\", $@"//{_remoteServerIp}/C$/");
                    searchPaths.Add(uncPathUnix);
                }
            }

            // Search in each path with each filename variation
            foreach (var searchPath in searchPaths)
            {
                try
                {
                    // Check if directory exists
                    if (!Directory.Exists(searchPath))
                    {
                        _logger.LogDebug("Directory does not exist: {SearchPath}", searchPath);
                        continue;
                    }

                    // Try each filename variation
                    foreach (var variation in fileNameVariations)
                    {
                        // Try direct file path
                        var fullPath = Path.Combine(searchPath, variation);
                        if (System.IO.File.Exists(fullPath))
                        {
                            _logger.LogInformation("Found file at path: {FullPath} (requested: {FileName})", fullPath, fileName);
                            return fullPath;
                        }

                        // Also try case-insensitive search
                        try
                        {
                            var foundFiles = Directory.GetFiles(searchPath, variation, SearchOption.TopDirectoryOnly);
                            if (foundFiles.Length > 0)
                            {
                                _logger.LogInformation("Found file via search: {FilePath} (requested: {FileName})", foundFiles[0], fileName);
                                return foundFiles[0];
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Error searching for {Variation} in directory: {SearchPath}", variation, searchPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error checking path: {SearchPath}", searchPath);
                }
            }

            _logger.LogWarning("File not found in any search path: {FileName}. Searched {VariationCount} variations in {PathCount} paths", 
                fileName, fileNameVariations.Count, searchPaths.Count);
            return null;
        }

        public async Task<LegacyMediaFileInfo?> GetMediaFileInfoAsync(string fileName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            _logger.LogInformation("Getting file info for: {FileName}", fileName);

            // Primary: Use API (soitmed_data_backend) - This is the most reliable since it runs on the same machine as files
            // soitmed_data_backend handles file search with multiple variations automatically
            if (!string.IsNullOrEmpty(_connectionSettings.LegacyMediaApiBaseUrl) && _httpClient.BaseAddress != null)
            {
                try
                {
                    var apiUrl = $"/api/Media/files/{Uri.EscapeDataString(fileName)}";
                    var fullUrl = _httpClient.BaseAddress + apiUrl;
                    _logger.LogInformation("Checking file existence via API: {FullUrl}", fullUrl);
                    
                    // Try HEAD request first (lighter)
                    var request = new HttpRequestMessage(HttpMethod.Head, apiUrl);
                    var response = await _httpClient.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentType = response.Content.Headers.ContentType?.MediaType ?? GetContentType(fileName);
                        var contentLength = response.Content.Headers.ContentLength;
                        var lastModified = response.Content.Headers.LastModified?.DateTime;

                        _logger.LogInformation("File exists via API: {FileName}, Size: {Size}, Type: {Type}", 
                            fileName, contentLength, contentType);

                        return new LegacyMediaFileInfo
                        {
                            FileName = fileName,
                            FileUrl = GetMediaFileUrl(fileName),
                            FileSize = contentLength,
                            ContentType = contentType,
                            Exists = true,
                            LastModified = lastModified
                        };
                    }
                    else
                    {
                        _logger.LogDebug("File not found via API HEAD: {FileName}, Status: {Status}. Trying GET request...", fileName, response.StatusCode);
                        
                        // If HEAD fails, try GET to see if file exists (some servers don't support HEAD)
                        var getRequest = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                        var getResponse = await _httpClient.SendAsync(getRequest, cancellationToken);
                        
                        if (getResponse.IsSuccessStatusCode)
                        {
                            var contentType = getResponse.Content.Headers.ContentType?.MediaType ?? GetContentType(fileName);
                            var contentLength = getResponse.Content.Headers.ContentLength;
                            var lastModified = getResponse.Content.Headers.LastModified?.DateTime;

                            _logger.LogInformation("File exists via API (GET): {FileName}, Size: {Size}, Type: {Type}", 
                                fileName, contentLength, contentType);

                            return new LegacyMediaFileInfo
                            {
                                FileName = fileName,
                                FileUrl = GetMediaFileUrl(fileName),
                                FileSize = contentLength,
                                ContentType = contentType,
                                Exists = true,
                                LastModified = lastModified
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting media file info via API: {FileName}. Will try direct access.", fileName);
                }
            }

            // Fallback: Try direct file system access (if running on same network/machine)
            var filePath = FindFileInLegacyPaths(fileName);
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    var contentType = GetContentType(fileName);
                    
                    _logger.LogInformation("File exists locally: {FilePath}, Size: {Size}", filePath, fileInfo.Length);

                    return new LegacyMediaFileInfo
                    {
                        FileName = fileName,
                        FileUrl = GetMediaFileUrl(fileName),
                        FileSize = fileInfo.Length,
                        ContentType = contentType,
                        Exists = true,
                        LastModified = fileInfo.LastWriteTime
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get file info from path: {FilePath}", filePath);
                }
            }

            _logger.LogWarning("File not found: {FileName}. Check if soitmed_data_backend is running on {ApiUrl}", 
                fileName, _connectionSettings.LegacyMediaApiBaseUrl);
            return new LegacyMediaFileInfo
            {
                FileName = fileName,
                FileUrl = GetMediaFileUrl(fileName),
                Exists = false
            };
        }

        /// <summary>
        /// Get content type based on file extension
        /// </summary>
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }
    }
}


