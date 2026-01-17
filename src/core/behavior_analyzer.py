import json
import os
import re

from .utils import get_config_path

class BehaviorAnalyzer:
    def __init__(self, config_filename="lolbins.json"):
        self.lolbins_config = {}
        self.load_config(get_config_path(config_filename))

    def load_config(self, config_path):
        if os.path.exists(config_path):
            try:
                with open(config_path, 'r') as f:
                    data = json.load(f)
                    # Convert list to dict for faster lookup by name
                    for tool in data.get("monitored_tools", []):
                        self.lolbins_config[tool["name"].lower()] = tool
            except Exception as e:
                print(f"Error loading lolbins config: {e}")

    def analyze_certutil(self, command_line):
        indicators = []
        cmd_lower = command_line.lower()
        if "urlcache" in cmd_lower:
            indicators.append("urlcache (Possible Download)")
        if "decode" in cmd_lower:
            indicators.append("decode (Obfuscation)")
        if "split" in cmd_lower:
            indicators.append("split (File Manipulation)")
        return indicators

    def analyze_powershell(self, command_line):
        indicators = []
        cmd_lower = command_line.lower()
        
        # Check for encoded command
        if "-encodedcommand" in cmd_lower or " -e " in cmd_lower or " -en " in cmd_lower:
            indicators.append("Encoded Command")
            
        # Check for hidden window
        if "-windowstyle hidden" in cmd_lower or "-w hidden" in cmd_lower:
            indicators.append("Hidden Window")
            
        # Check for execution/download keywords
        if "invoke-expression" in cmd_lower or "iex" in cmd_lower:
            indicators.append("Invoke-Expression")
        if "downloadstring" in cmd_lower or "downloadfile" in cmd_lower:
            indicators.append("Download Attempt")
        if "-noprofile" in cmd_lower:
            indicators.append("NoProfile")
        if "-noninteractive" in cmd_lower:
            indicators.append("NonInteractive")
            
        return indicators

    def analyze_mshta(self, command_line):
        indicators = []
        cmd_lower = command_line.lower()
        
        # Check for URL (http/https)
        if "http://" in cmd_lower or "https://" in cmd_lower:
            indicators.append("Remote Scriptlet Execution")
            
        if "javascript:" in cmd_lower or "vbscript:" in cmd_lower:
            indicators.append("Direct Code Execution")
            
        return indicators

    def analyze_wmic(self, command_line):
        indicators = []
        cmd_lower = command_line.lower()
        
        if "process call create" in cmd_lower:
            indicators.append("Process Creation")
        if "/node:" in cmd_lower:
            indicators.append("Remote Execution")
            
        return indicators
        
    def analyze_generic(self, tool_name, command_line):
        """Generic analysis based on JSON config for tools not explicitly handled"""
        indicators = []
        tool_config = self.lolbins_config.get(tool_name.lower())
        if not tool_config:
            return []
            
        cmd_lower = command_line.lower()
        for indicator in tool_config.get("threat_indicators", []):
            if indicator.lower() in cmd_lower:
                indicators.append(indicator)
                
        return indicators

    def calculate_threat_level(self, indicators, tool_name):
        if not indicators:
            return "safe"
            
        tool_config = self.lolbins_config.get(tool_name.lower())
        base_risk = tool_config.get("risk_level", "low") if tool_config else "low"
        
        # Simple logic: if we have indicators, it's at least the base risk.
        # If many indicators, elevate? 
        # For now, following SOP: "Classify threat level"
        
        if len(indicators) >= 2 or base_risk == "high":
             if base_risk == "high":
                 return "high" # Or critical if very suspicious
             return "medium"
             
        return base_risk

    def is_suspicious(self, process_info):
        """
        Main entry point for analysis.
        process_info dict expected: {'name': '...', 'cmdline': '...'}
        Returns: (is_suspicious, threat_level, indicators)
        """
        name = process_info.get('name', '').lower()
        cmdline = process_info.get('cmdline', '') or ""
        
        indicators = []
        
        # Dispatch to specific analyzers or generic
        if name == 'certutil.exe':
            indicators = self.analyze_certutil(cmdline)
        elif name == 'powershell.exe':
            indicators = self.analyze_powershell(cmdline)
        elif name == 'mshta.exe':
            indicators = self.analyze_mshta(cmdline)
        elif name == 'wmic.exe':
            indicators = self.analyze_wmic(cmdline)
        else:
            # Check other configured tools
            if name in self.lolbins_config:
                indicators = self.analyze_generic(name, cmdline)
                
        if indicators:
            threat_level = self.calculate_threat_level(indicators, name)
            return True, threat_level, indicators
            
        return False, "safe", []
