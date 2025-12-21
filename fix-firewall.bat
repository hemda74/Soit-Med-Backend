@echo off
echo ========================================
echo SoitMed API - Firewall Configuration
echo ========================================
echo.
echo Adding firewall rule to allow port 5117...
echo.

netsh advfirewall firewall add rule name="SoitMed API Port 5117" dir=in action=allow protocol=TCP localport=5117

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ SUCCESS: Firewall rule added!
    echo.
    echo The backend should now be accessible from Android devices.
    echo.
) else (
    echo.
    echo ❌ ERROR: Failed to add firewall rule.
    echo.
    echo Please run this script as Administrator:
    echo Right-click this file and select "Run as administrator"
    echo.
)

pause




