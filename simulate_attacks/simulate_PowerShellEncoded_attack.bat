@echo off
echo ==================================================
echo      WatchTower Attack Simulation
echo ==================================================
echo.
echo This script will execute a PowerShell command with
echo Base64 encoding, which should be detected by
echo the "PowerShell Encoded Command" rule.
echo.
echo Command: Start-Sleep -s 60
echo.
echo Launching PowerShell...
echo.

powershell.exe -NoProfile -EncodedCommand UwB0AGEAcgB0AC0AUwBsAGUAZQBwACAALQBzACAANgAwAA==

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
)
pause
