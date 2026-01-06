# How to Start soitmed_data_backend on Windows

## Problem
The `soitmed_data_backend` API is not running, so files cannot be retrieved via API.

## Solution: Start soitmed_data_backend

### Option 1: Run from Visual Studio / Command Line

1. **Navigate to soitmed_data_backend directory:**
   ```powershell
   cd C:\path\to\soitmed_data_backend
   ```

2. **Restore dependencies:**
   ```powershell
   dotnet restore
   ```

3. **Run the application:**
   ```powershell
   dotnet run
   ```

   Or specify the port:
   ```powershell
   dotnet run --urls "http://0.0.0.0:5266"
   ```

### Option 2: Run as Windows Service

1. **Create a service (requires admin):**
   ```powershell
   # Install as service
   sc create SoitMedDataBackend binPath="C:\path\to\soitmed_data_backend\SoitMed.DataBackend.exe" start=auto
   
   # Start the service
   sc start SoitMedDataBackend
   ```

### Option 3: Run in Background (PowerShell)

```powershell
# Start in background
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd C:\path\to\soitmed_data_backend; dotnet run"
```

## Verify it's Running

```powershell
# Check if port 5266 is listening
netstat -an | findstr 5266

# Test the API
Invoke-WebRequest -Uri "http://localhost:5266/api/Media/files/_20170102_151903.JPG" -Method GET
```

## Configure Firewall (if needed)

If you get "Access is denied", run PowerShell as Administrator:

```powershell
New-NetFirewallRule -DisplayName "SoitMed Data Backend" -Direction Inbound -LocalPort 5266 -Protocol TCP -Action Allow
```

## Alternative: Use Direct File Access

If `soitmed_data_backend` cannot be started, the backend will try to access files directly via UNC paths. However, this may not work on macOS/Linux.

For best results, **start soitmed_data_backend** on the Windows server.

