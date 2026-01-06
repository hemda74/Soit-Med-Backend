# Setup Instructions for Remote Device (IP: 10.10.9.104)

## Overview
To allow the main backend server to access media files from the remote device (10.10.9.104), you need to configure file sharing and network access.

## Required Setup on Remote Device (10.10.9.104)

### 1. Enable File Sharing (Windows)

#### Option A: Share Specific Folders
1. Right-click on the folder: `D:\Soit-Med\legacy\SOIT`
2. Select **Properties** → **Sharing** tab
3. Click **Share...**
4. Add user: `Everyone` (or specific user)
5. Set permissions: **Read** (at minimum)
6. Click **Share**

#### Option B: Enable Administrative Shares (Recommended)
1. Open **Registry Editor** (regedit)
2. Navigate to: `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System`
3. Create DWORD: `LocalAccountTokenFilterPolicy` = `1`
4. Restart the computer

This enables access to `\\10.10.9.104\D$` (administrative share)

### 2. Configure Windows Firewall

#### Allow File and Printer Sharing:
1. Open **Windows Defender Firewall**
2. Click **Allow an app or feature through Windows Firewall**
3. Check **File and Printer Sharing** for both **Private** and **Public** networks
4. Click **OK**

#### Allow SMB Ports (if needed):
1. Open **Windows Defender Firewall with Advanced Security**
2. Click **Inbound Rules** → **New Rule**
3. Select **Port** → **Next**
4. Select **TCP**, enter ports: `445, 139`
5. Allow the connection
6. Apply to all profiles
7. Name: "SMB File Sharing"

### 3. Configure Network Discovery

1. Open **Network and Sharing Center**
2. Click **Change advanced sharing settings**
3. Enable:
   - **Network discovery**
   - **File and printer sharing**
   - **Turn on automatic setup of network connected devices**

### 4. Test Network Access

From the main server, test UNC path access:
```bash
# Test if you can access the share
ping 10.10.9.104

# On Windows, test in File Explorer:
\\10.10.9.104\D$
```

### 5. Configure soitmed_data_backend

Make sure `soitmed_data_backend` is running and accessible:

1. **Check if service is running:**
   ```bash
   # Check if port 5266 is listening
   netstat -an | findstr 5266
   ```

2. **Verify appsettings.json:**
   ```json
   {
     "MediaRootPath": "D:\\Soit-Med\\legacy\\SOIT"
   }
   ```

3. **Test the API directly:**
   ```bash
   curl http://10.10.9.104:5266/api/Media/files/test.jpg
   ```

## Alternative: Use Mapped Network Drive

If UNC paths don't work, you can map a network drive:

1. On the main server (where backend runs):
   ```bash
   # Map network drive
   net use Z: \\10.10.9.104\D$ /persistent:yes
   ```

2. Update `appsettings.json`:
   ```json
   {
     "ConnectionSettings": {
       "LegacyMediaPaths": "Z:\\Soit-Med\\legacy\\SOIT\\Ar\\MNT\\FileUploaders\\Reports,Z:\\Soit-Med\\legacy\\SOIT\\UploadFiles\\Files"
     }
   }
   ```

## Security Considerations

### Recommended: Use Specific User Account

Instead of `Everyone`, create a dedicated service account:

1. Create user: `soitmed_service`
2. Set password (never expires)
3. Grant read-only access to media folders
4. Use this account for network access

### Firewall Rules

Only allow access from specific IPs:
- Main backend server IP
- Development machines (if needed)

## Troubleshooting

### Cannot Access UNC Path
1. Check if remote device is on same network
2. Verify firewall rules
3. Test with: `ping 10.10.9.104`
4. Try accessing from File Explorer: `\\10.10.9.104\D$`

### Permission Denied
1. Check folder permissions
2. Verify user account has read access
3. Check if administrative shares are enabled

### Connection Timeout
1. Check network connectivity
2. Verify firewall allows SMB (ports 445, 139)
3. Check if remote device is online

### Files Not Found
1. Verify files exist in the specified paths
2. Check file permissions
3. Ensure paths are correct in `appsettings.json`

## Testing

After setup, test from main server:

```bash
# Test file access
curl -X GET "http://localhost:5117/api/LegacyMedia/test/074477c6d8164e17af7967847fdfbb97.jfif" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

Expected result:
- ✅ File exists: true
- ✅ File download: success

