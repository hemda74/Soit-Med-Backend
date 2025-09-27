# 🚨 SoitMed Backend Troubleshooting Guide

## **Why Your App Stops Running After 1-3 Minutes**

### **🔍 Common Causes & Solutions**

---

## **1. Development Server Timeout**

**Problem**: Development servers have built-in timeouts
**Solution**:

- Keep the terminal window open
- Don't minimize or close the terminal
- Use the auto-restart scripts provided

---

## **2. Database Connection Issues**

**Problem**: Database connection drops or times out
**Symptoms**:

- `SqlException` errors
- Connection timeout messages
- Database-related crashes

**Solutions**:

```bash
# Check if SQL Server is running
net start MSSQL$SQLEXPRESS

# Test connection
sqlcmd -S DESKTOP-DTSVDRG\SQLEXPRESS -E
```

**Fix Connection String**:

```json
{
	"ConnectionStrings": {
		"DefaultConnection": "Server=DESKTOP-DTSVDRG\\SQLEXPRESS;Database=ITIWebApi44;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30;Command Timeout=30;"
	}
}
```

---

## **3. Memory Issues**

**Problem**: Application runs out of memory
**Symptoms**:

- `OutOfMemoryException`
- Slow performance before crash
- High memory usage in Task Manager

**Solutions**:

- Close other applications
- Add memory monitoring
- Check for memory leaks in code

---

## **4. Unhandled Exceptions**

**Problem**: Exceptions crash the application
**Symptoms**:

- Application stops without warning
- Error messages in console
- No graceful error handling

**Solutions**:

- Use the improved `Program.cs` with exception handling
- Add try-catch blocks
- Implement proper logging

---

## **5. Port Conflicts**

**Problem**: Port 5117 is already in use
**Symptoms**:

- `AddressAlreadyInUseException`
- "Port is already in use" error

**Solutions**:

```bash
# Check what's using port 5117
netstat -ano | findstr :5117

# Kill the process using the port
taskkill /PID <PID_NUMBER> /F

# Or use a different port
dotnet run --urls "http://localhost:5118"
```

---

## **🛠️ Quick Fixes**

### **Option 1: Use Auto-Restart Scripts**

```bash
# Windows Batch
keep-running.bat

# PowerShell
.\keep-running.ps1
```

### **Option 2: Use Improved Program.cs**

1. Replace your `Program.cs` with `Program_Improved.cs`
2. This includes better error handling and logging

### **Option 3: Run with Verbose Logging**

```bash
dotnet run --urls "http://localhost:5117" --verbosity detailed
```

### **Option 4: Run in Background (Windows)**

```bash
# Start in background
start /B dotnet run --urls "http://localhost:5117"

# Or use PowerShell
Start-Process -NoNewWindow dotnet -ArgumentList "run --urls http://localhost:5117"
```

---

## **🔧 Advanced Troubleshooting**

### **Check Application Logs**

```bash
# Enable detailed logging
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT=Debug
dotnet run --urls "http://localhost:5117"
```

### **Monitor Resource Usage**

1. Open Task Manager
2. Check CPU and Memory usage
3. Look for any spikes before crashes

### **Database Health Check**

```sql
-- Check database status
SELECT name, state_desc FROM sys.databases WHERE name = 'ITIWebApi44'

-- Check for blocking processes
SELECT * FROM sys.dm_exec_requests WHERE blocking_session_id <> 0
```

### **Network Connectivity**

```bash
# Test if port is accessible
telnet localhost 5117

# Check if SQL Server is accessible
telnet DESKTOP-DTSVDRG 1433
```

---

## **🚀 Recommended Solutions**

### **1. Immediate Fix - Use Auto-Restart**

```bash
# Run this command in your project directory
.\keep-running.bat
```

### **2. Long-term Fix - Improve Error Handling**

1. Replace `Program.cs` with `Program_Improved.cs`
2. Add proper exception handling
3. Implement health checks

### **3. Production-Ready Setup**

```bash
# Run as Windows Service
sc create "SoitMed Backend" binPath="C:\path\to\your\app\SoitMed.exe"
sc start "SoitMed Backend"
```

---

## **📊 Monitoring Commands**

### **Check if App is Running**

```bash
# Check if port is listening
netstat -ano | findstr :5117

# Check process
tasklist | findstr dotnet
```

### **View Real-time Logs**

```bash
# Run with detailed logging
dotnet run --urls "http://localhost:5117" --verbosity detailed --environment Development
```

### **Test API Health**

```bash
# Test health endpoint
curl http://localhost:5117/health

# Test Swagger
curl http://localhost:5117/swagger
```

---

## **🎯 Quick Start - Keep App Running**

1. **Open PowerShell as Administrator**
2. **Navigate to your project directory**
3. **Run the auto-restart script**:
      ```powershell
      .\keep-running.ps1
      ```
4. **Keep the PowerShell window open**

This will automatically restart your app if it crashes!

---

## **📞 Still Having Issues?**

If the app still stops running, check:

1. **Windows Event Viewer** for system errors
2. **SQL Server Error Logs** for database issues
3. **Application Logs** in the console output
4. **Resource Usage** in Task Manager

The most common cause is **database connection timeouts** - make sure SQL Server is running and accessible!

---

**Happy Coding! 🚀**
