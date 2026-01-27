@echo off
echo ==================================================
echo      WatchTower Attack Simulation: CertUtil
echo ==================================================
echo.
echo This script simulates a "CertUtil Download" attack.
echo It attempts to use certutil.exe to download a file from the web.
echo.
echo Command: certutil -urlcache -split -f https://google.com temp_test_file.txt
echo.
echo Launching CertUtil...
echo.

certutil -urlcache -split -f https://google.com temp_test_file.txt

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
    del temp_test_file.txt >nul 2>&1
)
pause
