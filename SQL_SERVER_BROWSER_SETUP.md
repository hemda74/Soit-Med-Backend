# SQL Server Browser Setup - REQUIRED for Named Instances

## ⚠️ CRITICAL: Enable SQL Server Browser Service

For named instances like `SQLEXPRESS`, the **SQL Server Browser** service MUST be running. This service helps clients find the correct port for named instances.

## Step-by-Step: Enable SQL Server Browser

### On the SQL Server Machine (10.10.9.104):

1. **Open Services:**
   - Press `Win + R`
   - Type: `services.msc`
   - Press Enter

2. **Find SQL Server Browser:**
   - Look for "SQL Server Browser" in the services list
   - It might be named "SQL Server Browser (MSSQLSERVER)" or similar

3. **Configure SQL Server Browser:**
   - Right-click "SQL Server Browser" → **Properties**
   - Set **Startup type** to **Automatic**
   - Click **Start** button
   - Click **OK**

4. **Verify it's running:**
   - The Status should show "Running"
   - If it doesn't start, check Windows Firewall (port 1434 UDP must be open)

## Alternative: Find the Actual Port

If you prefer NOT to use SQL Server Browser, find the actual port:

### Method 1: SQL Server Configuration Manager

1. Open **SQL Server Configuration Manager**
2. Go to: **SQL Server Network Configuration** → **Protocols for SQLEXPRESS**
3. Right-click **TCP/IP** → **Properties**
4. Go to **IP Addresses** tab
5. Scroll to **IPAll** section
6. Note the **TCP Dynamic Ports** value (e.g., 49152) OR **TCP Port** value
7. Update connection string: `Server=10.10.9.104\SQLEXPRESS,PORT_NUMBER`

### Method 2: SQL Query (If Connected via SSMS)

Run this query in SSMS:

```sql
SELECT 
    local_net_address,
    local_tcp_port
FROM sys.dm_exec_connections
WHERE session_id = @@SPID;
```

## Connection String Formats

### With SQL Server Browser (Recommended):
```
Server=10.10.9.104\SQLEXPRESS;Database=ITIWebApi44;...
```

### With Specific Port:
```
Server=10.10.9.104\SQLEXPRESS,49152;Database=ITIWebApi44;...
```
(Replace 49152 with your actual port)

## Firewall Configuration

If SQL Server Browser is enabled, ensure Windows Firewall allows:
- **Port 1434 UDP** (for SQL Server Browser)
- **Port 1433 TCP** (or your dynamic port) (for SQL Server)

### Add Firewall Rule (Run as Administrator):

```powershell
# Allow SQL Server Browser (UDP 1434)
netsh advfirewall firewall add rule name="SQL Server Browser" dir=in action=allow protocol=UDP localport=1434

# Allow SQL Server (TCP - replace PORT with actual port)
netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433
```

## Test Connection

After enabling SQL Server Browser, test from your development machine:

```powershell
cd D:\Soit-Med\Backend\SoitMed
dotnet run
```

Expected: Application should connect successfully.

