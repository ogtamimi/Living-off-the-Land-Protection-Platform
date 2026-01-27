rule Suspicious_CMD_Renamed {
    meta:
        description = "Detects CMD.exe based on strings even if renamed"
        author = "OGT WatchTower"
        level = "high"
    strings:
        $s1 = "Microsoft Corporation. All rights reserved." wide
        $s2 = "cmd.exe" wide fullword
        $s3 = "ComSpec" wide
    condition:
        all of them
}

rule Suspicious_Powershell_Renamed {
    meta:
        description = "Detects PowerShell based on strings"
        author = "OGT WatchTower"
        level = "high"
    strings:
        $s1 = "PowerShell" wide
        $s2 = "System.Management.Automation" ascii
    condition:
        any of them
}
