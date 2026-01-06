# Testing Legacy Media Endpoints

## Quick Start

### 1. Start the Backend Server

```bash
cd backend/SoitMed
./start-server.sh
```

Or manually:
```bash
dotnet run --launch-profile http
```

The server will run on:
- **Port 5117** (http profile) - `http://localhost:5117`
- **Port 58868** (IIS Express profile) - `http://localhost:58868`

### 2. Get JWT Token

You need a JWT token to access the endpoints. You can get it by:

1. **From Frontend:**
   - Log in through the web application
   - Open browser DevTools (F12)
   - Go to Application > Local Storage
   - Copy the `token` value

2. **From API:**
   ```bash
   curl -X POST "http://localhost:5117/api/Auth/login" \
     -H "Content-Type: application/json" \
     -d '{"email":"your@email.com","password":"yourpassword"}'
   ```

### 3. Set Environment Variable

```bash
export JWT_TOKEN="your_jwt_token_here"
```

### 4. Run Tests

#### Option A: Use the Test Script

```bash
cd backend/SoitMed
./test-media.sh
```

Test a specific file:
```bash
./test-media.sh 074477c6d8164e17af7967847fdfbb97.jfif
```

#### Option B: Manual curl Commands

**1. Verify Connection:**
```bash
curl -X GET "http://localhost:5117/api/LegacyMedia/verify" \
  -H "Authorization: Bearer $JWT_TOKEN"
```

**2. Test File Access:**
```bash
curl -X GET "http://localhost:5117/api/LegacyMedia/test/074477c6d8164e17af7967847fdfbb97.jfif" \
  -H "Authorization: Bearer $JWT_TOKEN"
```

**3. Check File Exists:**
```bash
curl -X GET "http://localhost:5117/api/LegacyMedia/check/074477c6d8164e17af7967847fdfbb97.jfif" \
  -H "Authorization: Bearer $JWT_TOKEN"
```

**4. Download File:**
```bash
curl -X GET "http://localhost:5117/api/LegacyMedia/files/074477c6d8164e17af7967847fdfbb97.jfif" \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -o test-file.jfif
```

## Endpoints

### 1. Verify Connection
```
GET /api/LegacyMedia/verify
```
Checks:
- Configuration settings
- Connection to `soitmed_data_backend` API
- API responsiveness

**Response:**
```json
{
  "success": true,
  "verification": {
    "timestamp": "2025-01-15T10:30:00Z",
    "configuration": {
      "legacyApiBaseUrl": "http://10.10.9.104:5266",
      "legacyMediaPaths": [
        "D:\\Soit-Med\\legacy\\SOIT\\Ar\\MNT\\FileUploaders\\Reports",
        "D:\\Soit-Med\\legacy\\SOIT\\UploadFiles\\Files"
      ],
      "configured": true
    },
    "tests": [
      {
        "test": "API Connection",
        "success": true,
        "message": "API responded with status: NotFound",
        "baseAddress": "http://10.10.9.104:5266",
        "statusCode": "NotFound"
      }
    ]
  }
}
```

### 2. Test File Access
```
GET /api/LegacyMedia/test/{fileName}
```
Tests:
- File existence via API
- File info retrieval
- File stream access

**Response:**
```json
{
  "success": true,
  "fileName": "074477c6d8164e17af7967847fdfbb97.jfif",
  "timestamp": "2025-01-15T10:30:00Z",
  "tests": [
    {
      "method": "API Check",
      "success": true,
      "message": "File found via API"
    },
    {
      "method": "Get File Info",
      "success": true,
      "message": "File exists. Size: 825780 bytes, Type: application/octet-stream",
      "fileInfo": {
        "exists": true,
        "fileSize": 825780,
        "contentType": "application/octet-stream",
        "fileUrl": "/api/LegacyMedia/files/074477c6d8164e17af7967847fdfbb97.jfif"
      }
    },
    {
      "method": "Get File Stream",
      "success": true,
      "message": "File stream retrieved successfully",
      "canRead": true
    }
  ]
}
```

### 3. Check File Exists
```
GET /api/LegacyMedia/check/{fileName}
```
**Response:**
```json
{
  "fileName": "074477c6d8164e17af7967847fdfbb97.jfif",
  "exists": true
}
```

### 4. Get File URL
```
GET /api/LegacyMedia/url/{fileName}
```
**Response:**
```json
{
  "fileName": "074477c6d8164e17af7967847fdfbb97.jfif",
  "url": "/api/LegacyMedia/files/074477c6d8164e17af7967847fdfbb97.jfif"
}
```

### 5. Download File
```
GET /api/LegacyMedia/files/{fileName}
```
Returns the actual file with appropriate content type.

## Troubleshooting

### Server Not Running

**Error:**
```
curl: (7) Failed to connect to localhost port 5117
```

**Solution:**
1. Start the server:
   ```bash
   cd backend/SoitMed
   dotnet run
   ```

2. Check if server is running on different port:
   ```bash
   # Try port 5117
   curl http://localhost:5117/health
   
   # Try port 58868
   curl http://localhost:58868/health
   ```

### Authentication Required

**Error:**
```
401 Unauthorized
```

**Solution:**
1. Get JWT token from frontend login
2. Set environment variable:
   ```bash
   export JWT_TOKEN="your_token"
   ```
3. Include in curl:
   ```bash
   -H "Authorization: Bearer $JWT_TOKEN"
   ```

### File Not Found

**Error:**
```
404 Not Found
```

**Possible Causes:**
1. File doesn't exist in legacy paths
2. `soitmed_data_backend` is not running
3. Network connectivity issues

**Solution:**
1. Check `soitmed_data_backend` is running:
   ```bash
   curl http://10.10.9.104:5266/health
   ```

2. Verify configuration in `appsettings.json`:
   ```json
   {
     "ConnectionSettings": {
       "LegacyMediaApiBaseUrl": "http://10.10.9.104:5266",
       "LegacyMediaPaths": "D:\\Soit-Med\\legacy\\SOIT\\Ar\\MNT\\FileUploaders\\Reports,D:\\Soit-Med\\legacy\\SOIT\\UploadFiles\\Files"
     }
   }
   ```

3. Check logs in backend console for detailed error messages

## Example Test File Names

From the contract data, example file names:
- `074477c6d8164e17af7967847fdfbb97.jfif`
- `726307a598914deabaf85ff481644d72.jpeg`
- `745d0cbbd8164d2da2d7761bcb5d7947.pdf`
- `MX-4140N_20211117_081649.pdf`

