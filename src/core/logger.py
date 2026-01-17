import logging
import os
import json
from datetime import datetime
import csv

class SecurityLogger:
    def __init__(self, log_dir="logs"):
        self.log_dir = log_dir
        if not os.path.exists(self.log_dir):
            os.makedirs(self.log_dir)
        
        self.log_file = os.path.join(self.log_dir, f"security_log_{datetime.now().strftime('%Y-%m-%d')}.json")
        self.error_log_file = os.path.join(self.log_dir, "error.log")
        
        # Setup error logging standard
        logging.basicConfig(filename=self.error_log_file, level=logging.ERROR, 
                            format='%(asctime)s - %(levelname)s - %(message)s')

    def _write_log(self, entry):
        """Append a log entry to the JSON log file."""
        # Read existing logs or start new list
        logs = []
        if os.path.exists(self.log_file):
            try:
                with open(self.log_file, 'r') as f:
                    logs = json.load(f)
            except (json.JSONDecodeError, ValueError):
                logs = []
        
        logs.append(entry)
        
        with open(self.log_file, 'w') as f:
            json.dump(logs, f, indent=4)

    def log_detection(self, process_info, threat_level):
        """Log a threat detection event."""
        entry = {
            "timestamp": datetime.now().isoformat(),
            "type": "DETECTION",
            "threat_level": threat_level,
            "process_info": process_info
        }
        self._write_log(entry)

    def log_action(self, pid, action, details=None):
        """Log an action taken by the system."""
        entry = {
            "timestamp": datetime.now().isoformat(),
            "type": "ACTION",
            "pid": pid,
            "action": action,
            "details": details or ""
        }
        self._write_log(entry)

    def log_error(self, error_message):
        """Log an error message."""
        logging.error(error_message)
        # Also log to main json for visibility in GUI if needed, or keep separate.
        # For now, sticking to logging module for errors.

    def export_logs(self, filepath, format="json"):
        """Export logs to a specific file."""
        if not os.path.exists(self.log_file):
            return False
            
        try:
            with open(self.log_file, 'r') as f:
                logs = json.load(f)
                
            if format.lower() == "json":
                with open(filepath, 'w') as f:
                    json.dump(logs, f, indent=4)
            elif format.lower() == "csv":
                if not logs:
                    return True
                
                keys = logs[0].keys()
                with open(filepath, 'w', newline='') as f:
                    writer = csv.DictWriter(f, fieldnames=keys)
                    writer.writeheader()
                    writer.writerows(logs)
            return True
        except Exception as e:
            self.log_error(f"Failed to export logs: {e}")
            return False

    def get_logs_by_date(self, date_str):
        """Get logs for a specific date (YYYY-MM-DD)."""
        target_file = os.path.join(self.log_dir, f"security_log_{date_str}.json")
        if os.path.exists(target_file):
            try:
                with open(target_file, 'r') as f:
                    return json.load(f)
            except:
                return []
        return []
        
    def get_all_logs(self):
        """Get current logs."""
        if os.path.exists(self.log_file):
             try:
                with open(self.log_file, 'r') as f:
                    return json.load(f)
             except:
                 return []
        return []
