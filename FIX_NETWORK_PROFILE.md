# 🔧 Fix: Network Profile Issue

## ❌ Problem Found

Your WiFi network is set to **Public** profile, which has stricter firewall rules that may block connections even with firewall rules enabled.

## ✅ Solution: Change Network to Private

### Option 1: Using PowerShell (Recommended)

1. **Open PowerShell as Administrator**
2. Run:
   ```powershell
   Get-NetConnectionProfile | Set-NetConnectionProfile -NetworkCategory Private
   ```

### Option 2: Using GUI

1. Open **Settings** → **Network & Internet** → **Wi-Fi**
2. Click on your WiFi network name
3. Under "Network profile", select **Private** (instead of Public)
4. Close settings

### Option 3: Use the Script

1. Right-click `Backend/fix-network-profile.ps1`
2. Select "Run with PowerShell" (as Administrator)
3. Follow the prompts

## 🔍 Why This Fixes It

- **Public networks** = Windows blocks most incoming connections (security)
- **Private networks** = Windows allows connections (trusted network)
- Your firewall rules work, but Public profile overrides them

## ✅ After Changing to Private

1. Test from Android browser: `http://10.10.9.104:5117/swagger`
2. Should work immediately!
3. No need to rebuild the app

## 📝 Verification

Check current profile:
```powershell
Get-NetConnectionProfile | Select Name, NetworkCategory
```

Should show: `NetworkCategory: Private`




