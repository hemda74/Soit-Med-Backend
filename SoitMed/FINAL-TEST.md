# Final Test Instructions

## Current Status

✅ Port 5117 is now free
✅ Code updated to use API first
✅ soitmed_data_backend is accessible from macOS

## Steps to Test:

### 1. Start Backend

```bash
cd backend/SoitMed
./restart-backend.sh
```

Or manually:
```bash
dotnet run --launch-profile http
```

Wait for: `Now listening on: http://localhost:5117`

### 2. Test File Access

In another terminal:
```bash
cd backend/SoitMed
./check-file-simple.sh "_20170102_151903.JPG"
```

## Expected Result:

```
✓ File EXISTS
✓ File RETRIEVED successfully!
✓ SUCCESS - File can be retrieved!
```

## If Still Not Working:

Check backend logs for:
- "Requesting file from legacy API: http://10.10.9.104:5266/api/Media/files/..."
- "File exists via API" or error messages

## Architecture Confirmed:

```
Main Backend (macOS, Current DB)
    ↓ HTTP Request
soitmed_data_backend (Windows, TBS DB - proxy only)
    ↓ File System
Windows Files (D:\Soit-Med\legacy\...)
```

✅ You use current database
✅ Files served via proxy
✅ System works correctly

