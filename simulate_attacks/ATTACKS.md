# ⚔️ Attack Simulators

This directory contains batch scripts designed to simulate various "Living-off-the-Land" (LOLBin) attacks. Use these scripts to verify that **OGT WatchTower** is correctly detecting and blocking malicious behavior.

## 📋 Available Simulations

### 1. PowerShell Encoded Command (`simulate_attack.bat`)
*   **Technique**: Obfuscation / Execution
*   **What it does**: Executes a Base64 encoded PowerShell command (`Start-Sleep -s 60`).
*   **Why malicious?**: Attackers use encoding to bypass string-based signature detection.
*   **Expected Result**: WatchTower should detect "PowerShell Encoded Command" and **TERMINATE** the process immediately.

### 2. PowerShell Download (`simulate_powershell_download.bat`)
*   **Technique**: Download / Command & Control
*   **What it does**: Uses `Invoke-WebRequest` to download a file from the internet.
*   **Why malicious?**: Often used as a "dropper" to download the next stage of malware.
*   **Expected Result**: WatchTower should detect "PowerShell Download" and **TERMINATE** or **SUSPEND** the process.

### 3. CertUtil Download (`simulate_certutil_download.bat`)
*   **Technique**: Ingress Tool Transfer (T1105)
*   **What it does**: Uses the built-in Windows certificate utility (`certutil.exe`) to download a file.
*   **Why malicious?**: `certutil` is a trusted binary, making it a popular tool for bypassing application whitelisting.
*   **Expected Result**: WatchTower should detect "Certutil Download" and **TERMINATE** the process.

### 4. MSHTA Network Connection (`simulate_mshta_network.bat`)
*   **Technique**: Signed Binary Proxy Execution (T1218.005)
*   **What it does**: Launches `mshta.exe` pointing to a remote URL (e.g., `http://127.0.0.1/evil.hta`).
*   **Why malicious?**: MSHTA can execute HTA files (HTML Applications) which have full access to the system.
*   **Expected Result**: WatchTower should detect "Mshta Network Activity" and **TERMINATE** the process.

### 5. WMIC Process Creation (`simulate_wmic_process.bat`)
*   **Technique**: Windows Management Instrumentation (T1047)
*   **What it does**: Uses `wmic` to spawn a new process (`calc.exe`).
*   **Why malicious?**: Attackers use WMIC for lateral movement and to spawn processes without using the standard API directly.
*   **Expected Result**: WatchTower should detect "WMIC Process Creation" and **TERMINATE** or **SUSPEND** the process.

### 6. Whoami Reconnaissance (`simulate_whoami.bat`)
*   **Technique**: System Owner/User Discovery (T1033)
*   **What it does**: Runs `whoami /priv` to check current user privileges.
*   **Why malicious?**: This is a standard "discovery" step attackers take immediately after compromising a machine to see if they have Admin rights.
*   **Expected Result**: WatchTower should detect "Whoami Execution" and **LOG** or **ALERT** on the activity (typically lower severity, but suspicious).

## ⚠️ Important Note
These scripts are **safe** simulations. They do not download actual malware or perform harmful actions. They simply use the *patterns* and *techniques* that real malware uses, to trigger the behavioral detection engine.
