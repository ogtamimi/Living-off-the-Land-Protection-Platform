@echo off
echo ==================================================
echo      WatchTower Attack Simulation: Whoami Recon
echo ==================================================
echo.
echo This script simulates a "Whoami" reconnaissance command.
echo Attackers often use whoami /priv or /all to check permissions.
echo.
echo Command: whoami /priv
echo.
echo Launching Whoami...
echo.

whoami /priv

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
