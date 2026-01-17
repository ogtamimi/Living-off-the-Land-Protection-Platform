import wmi
import psutil
import threading
import pythoncom
import time
import json
import logging
import os

from .utils import get_config_path

class ProcessMonitor:
    def __init__(self, event_callback=None, config_filename="lolbins.json"):
        """
        event_callback: function to call when a relevant process starts. 
                        Should accept (process_info_dict).
        """
        self.monitoring = False
        self.monitor_thread = None
        self.event_callback = event_callback
        self.monitored_apps = set()
        self.load_config(get_config_path(config_filename))

    def load_config(self, config_path):
        if os.path.exists(config_path):
            try:
                with open(config_path, 'r') as f:
                    data = json.load(f)
                    for tool in data.get("monitored_tools", []):
                        if tool.get("enabled", True):
                            self.monitored_apps.add(tool["name"].lower())
            except Exception as e:
                logging.error(f"Error loading lolbins for monitor: {e}")
        
        # Fallback if config failed or empty
        if not self.monitored_apps:
            logging.warning("Config empty or failed. Using hardcoded defaults.")
            defaults = ["certutil.exe", "powershell.exe", "mshta.exe", "wmic.exe", "bitsadmin.exe", "regsvr32.exe"]
            for d in defaults:
                self.monitored_apps.add(d)
        
        # FORCE ADD common apps for demonstration purposes
        # This ensures they appear even if the user has an old config file
        demo_apps = ["chrome.exe", "devenv.exe", "code.exe", "notepad.exe", "firefox.exe", "msedge.exe"]
        for app in demo_apps:
            self.monitored_apps.add(app)

    def start_monitoring(self):
        if self.monitoring:
            return
        self.monitoring = True
        self.monitor_thread = threading.Thread(target=self._monitor_loop, daemon=True)
        self.monitor_thread.start()

    def stop_monitoring(self):
        self.monitoring = False
        # WMI watcher is blocking, so we might need to handle stopping gracefully 
        # or just let the daemon thread die with the app.
        # Ideally, we cancel the watcher, but Python WMI is tricky.
        # We will check self.monitoring in the loop if we used polling, 
        # but for WMI event, it blocks. 
        # A common pattern is to just let it run or rely on the thread being daemon.

    def _monitor_loop(self):
        pythoncom.CoInitialize() # Required for WMI in thread
        c = wmi.WMI()
        watcher = c.Win32_ProcessStartTrace.watch_for("ProcessStart")
        
        while self.monitoring:
            try:
                # Timed wait to allow checking self.monitoring flag periodically
                process = watcher(timeout_ms=1000) 
                
                # Check filter - DISABLED to monitor ALL apps as requested
                # if process.ProcessName.lower() in self.monitored_apps:
                self._handle_process_event(process)
                    
            except wmi.x_wmi_timed_out:
                continue
            except Exception as e:
                logging.error(f"Error in monitoring loop: {e}")
                time.sleep(1) # Prevent tight loop on error
        
        pythoncom.CoUninitialize()

    def _handle_process_event(self, wmi_process):
        try:
            pid = wmi_process.ProcessID
            name = wmi_process.ProcessName
            
            # WMI StartTrace doesn't always have CommandLine. 
            # We need to query Win32_Process or use psutil to get it.
            cmdline = self.get_process_command_line(pid)
            
            info = {
                "pid": pid,
                "name": name,
                "cmdline": cmdline,
                "timestamp": time.time()
            }
            
            if self.event_callback:
                self.event_callback(info)
                
        except Exception as e:
            logging.error(f"Error handling process event: {e}")

    def get_process_command_line(self, pid):
        try:
            # Try psutil first as it's fast
            try:
                p = psutil.Process(pid)
                return " ".join(p.cmdline())
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                pass
                
            # Fallback to WMI if psutil fails (sometimes WMI has more rights or different view)
            # Note: Already CoInitialized in thread, but if called from outside, might need it.
            # Assuming this is called from thread or context where WMI works.
            # However, safer to use psutil generally. 
            pass
        except Exception:
            pass
        return ""

    def get_running_lolbins(self):
        """Scan currently running processes for LOLBins."""
        found = []
        for p in psutil.process_iter(['pid', 'name', 'cmdline']):
            try:
                if p.info['name'].lower() in self.monitored_apps:
                    found.append({
                        "pid": p.info['pid'],
                        "name": p.info['name'],
                        "cmdline": " ".join(p.info['cmdline'] or []),
                        "timestamp": p.create_time()
                    })
            except (psutil.NoSuchProcess, psutil.AccessDenied):
                continue
        return found
    
    def get_process_details(self, pid):
        try:
            p = psutil.Process(pid)
            return {
                "pid": pid,
                "name": p.name(),
                "status": p.status(),
                "cpu_percent": p.cpu_percent(),
                "memory_info": p.memory_info(),
                "create_time": p.create_time()
            }
        except Exception as e:
            return {}
