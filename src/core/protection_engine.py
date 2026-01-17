import psutil
import json
import os
import logging

from .utils import get_config_path

class ProtectionEngine:
    def __init__(self, config_filename="settings.json"):
        self.config_file = get_config_path(config_filename)
        self.whitelist = []
        self.load_settings()

    def load_settings(self):
        if os.path.exists(self.config_file):
            try:
                with open(self.config_file, 'r') as f:
                    data = json.load(f)
                    self.whitelist = data.get("whitelist", [])
            except Exception as e:
                logging.error(f"Error loading whitelist: {e}")

    def save_settings(self):
        # We only really update whitelist here ideally
        if os.path.exists(self.config_file):
            try:
                with open(self.config_file, 'r') as f:
                    data = json.load(f)
                
                data["whitelist"] = self.whitelist
                
                with open(self.config_file, 'w') as f:
                    json.dump(data, f, indent=4)
            except Exception as e:
                logging.error(f"Error saving settings: {e}")

    def suspend_process(self, pid):
        try:
            p = psutil.Process(pid)
            p.suspend()
            return True, "Process suspended successfully"
        except psutil.NoSuchProcess:
            return False, "Process does not exist"
        except psutil.AccessDenied:
            return False, "Access denied (Admin rights required?)"
        except Exception as e:
            return False, f"Error: {str(e)}"

    def terminate_process(self, pid):
        try:
            p = psutil.Process(pid)
            p.terminate()
            return True, "Process terminated successfully"
        except psutil.NoSuchProcess:
            return False, "Process does not exist"
        except psutil.AccessDenied:
            return False, "Access denied"
        except Exception as e:
            return False, f"Error: {str(e)}"

    def is_whitelisted(self, process_path):
        # Simple string match for now, could be improved to checksum
        if not process_path:
            return False
        return process_path in self.whitelist

    def add_to_whitelist(self, process_path):
        if process_path and process_path not in self.whitelist:
            self.whitelist.append(process_path)
            self.save_settings()
            return True
        return False

    def quarantine_process(self, pid):
        """
        'Quarantine' in the context of a running process usually implies 
        immediate suspension and preventing interaction.
        For LOLBins (system files), we cannot move the file itself.
        So we will suspend it.
        """
        return self.suspend_process(pid)
