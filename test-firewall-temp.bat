@echo off
echo ========================================
echo TEMPORARY FIREWALL TEST
echo ========================================
echo.
echo This will TEMPORARILY disable firewall for testing
echo Press Ctrl+C to cancel, or
pause
echo.
echo Disabling firewall temporarily...
netsh advfirewall set allprofiles state off
echo.
echo ✅ Firewall DISABLED
echo.
echo NOW TEST from your Android device:
echo http://10.10.9.104:5117/swagger
echo.
echo Press any key to RE-ENABLE firewall...
pause
echo.
echo Re-enabling firewall...
netsh advfirewall set allprofiles state on
echo.
echo ✅ Firewall RE-ENABLED
echo.
pause




