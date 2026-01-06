# Quick Fix: Restart Backend

## Problem
Backend returns "File NOT FOUND" even though:
- ✅ `soitmed_data_backend` is running and accessible
- ✅ File exists on Windows
- ✅ Direct API test works

## Solution: Restart Backend

The code has been updated to use API first, but the backend needs to be restarted.

### Steps:

1. **Stop backend** (Ctrl+C in terminal where it's running)

2. **Restart backend**:
   ```bash
   cd backend/SoitMed
   dotnet run
   ```

3. **Wait for**: `Now listening on: http://localhost:5117`

4. **Test**:
   ```bash
   ./check-file-simple.sh "_20170102_151903.JPG"
   ```

## What Changed:

- ✅ API is now PRIMARY method (not fallback)
- ✅ Handles HEAD method not supported (tries GET)
- ✅ Better timeout handling
- ✅ Improved error logging

## Expected Result:

```
✓ File EXISTS
✓ File RETRIEVED successfully!
```

## Architecture:

```
Main Backend (macOS) 
    ↓ (HTTP Request)
soitmed_data_backend (Windows: 10.10.9.104:5266)
    ↓ (File System Access)
Windows File System (D:\Soit-Med\legacy\...)
```

- Main backend uses: **Current Database (ITIWebApi44)**
- soitmed_data_backend uses: **TBS Database** (for file serving only)
- Files are served via proxy through soitmed_data_backend

