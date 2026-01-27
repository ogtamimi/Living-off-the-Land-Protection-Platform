@echo off
echo ==================================================
echo      WatchTower Attack Simulation: WMIC Process
echo ==================================================
echo.
echo This script simulates a "WMIC Process Creation" attack.
echo Attackers use WMIC to spawn processes laterally or locally.
echo.
echo Command: wmic process call create "calc.exe"
echo.
echo Launching WMIC...
echo.

wmic process call create "calc.exe"

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
