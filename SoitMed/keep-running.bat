@echo off
echo Starting SoitMed Backend with Auto-Restart...
echo Press Ctrl+C to stop

:loop
echo.
echo [%date% %time%] Starting application...
dotnet run --urls "http://localhost:5117"

echo.
echo [%date% %time%] Application stopped. Restarting in 5 seconds...
timeout /t 5 /nobreak > nul
goto loop
