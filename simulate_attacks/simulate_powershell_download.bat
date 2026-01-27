@echo off
echo ==================================================
echo      WatchTower Attack Simulation: PowerShell Download
echo ==================================================
echo.
echo This script simulates a "PowerShell Download" attack.
echo It attempts to use powershell.exe to download a file from the web.
echo.
echo Command: powershell -c "Invoke-WebRequest -Uri http://google.com -OutFile temp_test.html"
echo.
echo Launching PowerShell...
echo.

powershell -c "Invoke-WebRequest -Uri http://google.com -OutFile temp_test.html"

if %errorlevel% neq 0 (
    echo.
    echo ==================================================
    echo [SUCCESS] The attack process was terminated!
    echo ==================================================
) else (
    echo.
    echo ==================================================
    echo If you see this message, the attack completed
    echo without being suspended/terminated.
    echo ==================================================
    del temp_test.html >nul 2>&1
)
pause
