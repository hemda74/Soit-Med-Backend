# Mobile Device Connection Guide

This guide explains how to connect your mobile device (phone/tablet) to the Soit-Med Backend API during development.

## Why You Need This

When testing your mobile app (React Native/Expo) on a physical device, you can't use `localhost` because that refers to the phone itself, not your development computer. You need to use your computer's **Local IP Address** instead.

## Step 1: Find Your Computer's Local IP Address

**Ensure both your computer and phone are connected to the same Wi-Fi network.**

### On Windows:

1. Open Command Prompt (`cmd`)
2. Type: `ipconfig`
3. Look for **IPv4 Address** (e.g., `192.168.1.10`)

### On macOS:

1. Open Terminal
2. Type: `ifconfig | grep "inet "`
3. Find the IP address that is NOT `127.0.0.1`

### On Linux:

1. Open Terminal
2. Type: `ip addr show` or `hostname -I`
3. Look for your local network IP (usually starts with `192.168.` or `10.0.`)

**Example:** Let's assume your IP is `192.168.1.10`

## Step 2: API Configuration (Already Done ✅)

The backend API has been configured to accept connections from external devices:

- **Application URL:** `http://0.0.0.0:5117`
- **Port:** 5117

The `0.0.0.0` address tells the server to listen on all available network interfaces, making it accessible from other devices on your network.

## Step 3: Update Your Mobile App Configuration

In your React Native/Expo app, update the API URL to use your computer's IP address:

### Before (Won't work on physical devices):

```javascript
const API_URL = 'http://localhost:5117/api';
```

### After (Works on physical devices):

```javascript
// Replace 192.168.1.10 with YOUR computer's actual IP
const API_URL = 'http://192.168.1.10:5117/api';
```

### Example API Calls:

```javascript
// Login endpoint
const response = await fetch('http://192.168.1.10:5117/api/Account/Login', {
	method: 'POST',
	headers: { 'Content-Type': 'application/json' },
	body: JSON.stringify({ email, password }),
});

// Get users
const users = await fetch('http://192.168.1.10:5117/api/User');
```

## Troubleshooting 🔧

### 1. Still Getting Network Error?

#### Windows Firewall (Most Common Issue)

Your firewall is likely blocking incoming connections:

1. Open **Windows Defender Firewall with Advanced Security**
2. Click **Inbound Rules** → **New Rule**
3. Select **Port** → Next
4. Select **TCP**, enter port `5117` → Next
5. Select **Allow the connection** → Next
6. Check all profiles (Domain, Private, Public) → Next
7. Name it "Soit-Med API Development" → Finish

#### Alternative: Quick Test (Temporarily disable firewall)

```powershell
# Run PowerShell as Administrator
# WARNING: This temporarily disables firewall
netsh advfirewall set allprofiles state off

# After testing, re-enable it
netsh advfirewall set allprofiles state on
```

### 2. Android Cleartext Traffic Error 🔴

Android 9+ blocks non-HTTPS traffic by default. For development, you need to explicitly allow it.

**For Expo Projects:**

Add this to your `app.json` or `app.config.js`:

```json
{
	"expo": {
		"android": {
			"usesCleartextTraffic": true
		}
	}
}
```

**For Native Android Projects:**

Add to `android/app/src/main/AndroidManifest.xml`:

```xml
<application
  android:usesCleartextTraffic="true"
  ...>
```

**Important:** After making this change:

- Restart your Expo development server
- Or rebuild the app: `npx expo prebuild`

### 3. Network Isolation

Some Wi-Fi networks (public, corporate, university) use "Client Isolation" which prevents devices from seeing each other.

**Solution:** Use your phone's mobile hotspot:

1. Enable hotspot on your phone
2. Connect your computer to the hotspot
3. Find your computer's new IP address (Step 1)
4. Update your app's API URL

### 4. Is the API Running?

Before testing, make sure:

- ✅ The .NET API is running (you should see Swagger at `http://localhost:5117/swagger`)
- ✅ No error messages in the terminal/console
- ✅ The port number matches (5117)

### 5. Test Connection from Computer First

Open your browser and navigate to:

```
http://YOUR_COMPUTER_IP:5117/swagger
```

Example: `http://192.168.1.10:5117/swagger`

If this doesn't work, the issue is with your network/firewall setup, not your mobile app.

## Quick Reference

| Component            | Value                                        |
| -------------------- | -------------------------------------------- |
| **Port**             | 5117                                         |
| **Protocol**         | HTTP (not HTTPS)                             |
| **Base URL Format**  | `http://YOUR_IP:5117/api`                    |
| **Example IP**       | 192.168.1.10                                 |
| **Full Example URL** | `http://192.168.1.10:5117/api/Account/Login` |

## Production Considerations ⚠️

**Important:** These settings are for **DEVELOPMENT ONLY**. In production:

- ✅ Use HTTPS with proper SSL certificates
- ✅ Remove `usesCleartextTraffic` from Android config
- ✅ Use a proper domain name instead of IP addresses
- ✅ Configure proper firewall rules
- ✅ Use environment variables for API URLs

## Environment-Based Configuration (Recommended)

Create a config file in your mobile app:

```javascript
// config/api.js
const getApiUrl = () => {
	if (__DEV__) {
		// Development: Use your computer's IP
		return 'http://192.168.1.10:5117/api';
	} else {
		// Production: Use your actual domain
		return 'https://api.soitmed.com/api';
	}
};

export const API_URL = getApiUrl();
```

Then import it everywhere:

```javascript
import { API_URL } from './config/api';

fetch(`${API_URL}/Account/Login`, {
	/* ... */
});
```

## Need Help?

If you're still experiencing issues:

1. Verify both devices are on the same network
2. Check the API is running (`http://localhost:5117/swagger` works on your computer)
3. Verify the IP address is correct (it can change)
4. Check firewall settings
5. Try the mobile hotspot method
6. Check for typos in the URL (common mistake!)

---

**Last Updated:** October 6, 2025

