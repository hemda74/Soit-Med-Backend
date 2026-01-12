# Legacy Media Files Integration

## Overview

This document describes the integration between the main SoitMed backend and the legacy media API (`soitmed_data_backend`) for serving legacy media files.

## Architecture

### Legacy Media API (`soitmed_data_backend`)

- **Port**: 5266 (configurable)
- **Base URL**: `http://10.10.9.104:5266` (or `http://localhost:5266` for local)
- **Purpose**: Serves legacy media files from the old SOIT system
- **Media Paths**:
     - `D:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports`
     - `D:\Soit-Med\legacy\SOIT\UploadFiles\Images`
     - `D:\Soit-Med\legacy\SOIT\UploadFiles\Files`

### Main SoitMed Backend Integration

- **Service**: `LegacyMediaService` - Proxies requests to legacy API
- **Controller**: `LegacyMediaController` - Exposes endpoints for legacy media
- **Configuration**: `ConnectionSettings.LegacyMediaApiBaseUrl`

## Configuration

### appsettings.json

```json
{
	"ConnectionSettings": {
		"Mode": "Remote",
		"LegacyMediaApiBaseUrl": "http://10.10.9.104:5266",
		"LegacyMediaPaths": "D:\\Soit-Med\\legacy\\SOIT\\Ar\\MNT\\FileUploaders\\Reports,D:\\Soit-Med\\legacy\\SOIT\\UploadFiles\\Images,D:\\Soit-Med\\legacy\\SOIT\\UploadFiles\\Files"
	}
}
```

## API Endpoints

### 1. Get Legacy Media File (Proxy)

```
GET /api/LegacyMedia/files/{fileName}
```

**Description**: Proxies request to legacy media API and returns the file.

**Example**:

```
GET /api/LegacyMedia/files/81d25aa6f0f946988e394f05fa351abf.jpg
```

**Response**: Returns the file with appropriate content type.

### 2. Get Media File URL

```
GET /api/LegacyMedia/url/{fileName}
```

**Description**: Returns the URL to access the legacy media file (does not download).

**Response**:

```json
{
	"fileName": "81d25aa6f0f946988e394f05fa351abf.jpg",
	"url": "http://10.10.9.104:5266/api/Media/files/81d25aa6f0f946988e394f05fa351abf.jpg"
}
```

### 3. Check Media File Exists

```
GET /api/LegacyMedia/check/{fileName}
```

**Description**: Checks if a legacy media file exists.

**Response**:

```json
{
	"fileName": "81d25aa6f0f946988e394f05fa351abf.jpg",
	"exists": true
}
```

## Usage in Code

### Getting Media File URL

```csharp
var legacyMediaService = serviceProvider.GetService<ILegacyMediaService>();
var fileUrl = legacyMediaService.GetMediaFileUrl("filename.jpg");
// Returns: "http://10.10.9.104:5266/api/Media/files/filename.jpg"
```

### Checking File Existence

```csharp
var exists = await legacyMediaService.CheckMediaFileExistsAsync("filename.jpg");
```

### Getting File Stream

```csharp
var stream = await legacyMediaService.GetMediaFileStreamAsync("filename.jpg");
if (stream != null)
{
    // Process file stream
}
```

## Legacy Data Import Integration

When importing legacy maintenance visits, file references can be stored in `MaintenanceRequestAttachment`:

```csharp
// Parse pipe-separated file names from legacy system
var fileNames = legacyVisit.Files?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

foreach (var fileName in fileNames)
{
    // Check if file exists in legacy system
    var exists = await _legacyMediaService.CheckMediaFileExistsAsync(fileName);

    if (exists)
    {
        // Create attachment record pointing to legacy file
        var attachment = new MaintenanceRequestAttachment
        {
            MaintenanceRequestId = maintenanceRequestId,
            FilePath = _legacyMediaService.GetMediaFileUrl(fileName), // Store URL
            FileName = fileName,
            FileType = GetContentTypeFromFileName(fileName),
            AttachmentType = GetAttachmentTypeFromFileName(fileName),
            Description = "Imported from legacy system",
            UploadedAt = legacyVisit.VisitingDate ?? DateTime.UtcNow,
            IsActive = true
        };

        await _unitOfWork.MaintenanceRequestAttachments.CreateAsync(attachment);
    }
}
```

## Frontend Integration

### React Dashboard

```typescript
// Get media file URL
const getLegacyMediaUrl = (fileName: string) => {
	return `/api/LegacyMedia/files/${encodeURIComponent(fileName)}`;
};

// Display image
<img
	src={getLegacyMediaUrl(attachment.fileName)}
	alt={attachment.fileName}
/>;
```

### React Native

```typescript
// Get media file URL
const getLegacyMediaUrl = (fileName: string) => {
	const baseUrl = 'http://10.10.9.104:58868'; // Main API base URL
	return `${baseUrl}/api/LegacyMedia/files/${encodeURIComponent(
		fileName
	)}`;
};

// Display image
<Image source={{ uri: getLegacyMediaUrl(attachment.fileName) }} />;
```

## Security Considerations

1. **Path Traversal Protection**: The service validates file names to prevent path traversal attacks
2. **Authentication**: All endpoints require authentication (`[Authorize]`)
3. **URL Encoding**: File names are properly URL-encoded
4. **Caching**: Files are cached for 24 hours to reduce load

## Error Handling

- **File Not Found**: Returns 404 with error message
- **API Unavailable**: Returns 500 with error details
- **Invalid File Name**: Returns 400 Bad Request

## Performance

- **Caching**: Files are cached for 24 hours
- **Range Requests**: Supports HTTP range requests for large files
- **Connection Pooling**: HttpClient is reused via IHttpClientFactory

## Troubleshooting

### File Not Found

1. Check if legacy media API is running
2. Verify `LegacyMediaApiBaseUrl` in appsettings.json
3. Check if file exists in legacy file system paths
4. Review legacy media API logs

### Connection Issues

1. Verify network connectivity to legacy API server
2. Check firewall rules
3. Verify legacy API is accessible from main backend server

## Future Enhancements

1. **File Migration**: Option to copy legacy files to new storage
2. **Thumbnail Generation**: Generate thumbnails for images
3. **CDN Integration**: Serve files through CDN for better performance
4. **File Synchronization**: Sync file availability status
