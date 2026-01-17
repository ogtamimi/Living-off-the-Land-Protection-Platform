import sys
import os
import ctypes
import queue
import threading
from tkinter import messagebox

# Add current directory to path
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from gui.main_window import MainWindow
from core.process_monitor import ProcessMonitor
from core.behavior_analyzer import BehaviorAnalyzer
from core.protection_engine import ProtectionEngine
from core.logger import SecurityLogger

class WatchTowerApp:
    def __init__(self):
        # 0. Check Admin
        if not self.is_admin():
            self.request_admin()
            sys.exit()

        # 1. Initialize Core Components
        self.logger = SecurityLogger()
        self.analyzer = BehaviorAnalyzer()
        self.protection = ProtectionEngine()
        
        # Event Queue for Thread Safety
        self.event_queue = queue.Queue()
        
        # Monitor
        self.monitor = ProcessMonitor(event_callback=self.handle_process_event)
        
        # 2. Initialize GUI
        self.window = MainWindow()
        self.window.on_protection_toggle = self.on_protection_toggle
        
        # Inject references
        self.window.panels["Logs"].set_logger(self.logger)
        
        # Stats tracking
        self.stats = {
            "monitored": 0,
            "threats": 0,
            "suspended": 0
        }
        
        # 3. Start Update Loop
        self.window.after(100, self.process_queue)
        
        # 4. Scan Existing Processes (So dashboard isn't empty)
        print("Scanning identifying existing processes...")
        initial_procs = self.monitor.get_running_lolbins()
        for p in initial_procs:
            self.process_logic(p)
            
        # 5. Start Monitoring
        self.logger.log_error("Application Started")
        try:
            self.monitor.start_monitoring()
        except Exception as e:
            messagebox.showerror("Error", f"Failed to start monitoring: {e}")
            self.logger.log_error(f"Startup Error: {e}")

    def is_admin(self):
        try:
            return ctypes.windll.shell32.IsUserAnAdmin()
        except:
            return False

    def request_admin(self):
        ctypes.windll.shell32.ShellExecuteW(
            None, "runas", sys.executable, " ".join(sys.argv), None, 1
        )

    def on_protection_toggle(self, active):
        # We might stop the monitor thread or just ignore events
        # Usually, protection toggle implies whether we ACT on threats, 
        # but we might still want to monitor/log.
        # SOP says "Protection toggle button".
        # I'll update the protection engine setting or just flag here.
        # Ideally, we update ProtectionEngine config? Or just a flag in App?
        # Let's assume it disables AUTOMATIC ACTIONS but still monitors.
        pass 

    def handle_process_event(self, process_info):
        self.event_queue.put(process_info)

    def process_queue(self):
        try:
            while not self.event_queue.empty():
                info = self.event_queue.get_nowait()
                self.process_logic(info)
        except queue.Empty:
            pass
        finally:
            self.window.after(100, self.process_queue)

    def process_logic(self, info):
        self.stats["monitored"] += 1
        
        # Analyze
        is_suspicious, threat_level, indicators = self.analyzer.is_suspicious(info)
        
        action_taken = "Monitored"
        
        if is_suspicious:
            self.stats["threats"] += 1
            info["behavior"] = ", ".join(indicators)
            info["threat_level"] = threat_level
            
            # Protection Logic
            if self.window.protection_active:
                # Check settings (reloading or cached)
                # Ideally, ProtectionEngine has the current config.
                # We can reload settings periodically or on event.
                # For simplicity, we assume ProtectionEngine has current cache 
                # (SettingsPanel saves to disk, ProtectionEngine loads from disk in init. 
                # We might need to refresh ProtectionEngine settings).
                self.protection.load_settings() # Refresh settings
                config = self.protection.whitelist # re-read whitelist
                # And protection settings... ProtectionEngine needs to read "auto_suspend" etc.
                # Actually ProtectionEngine in current implementation only handles Whitelist in settings.
                # But settings.json has "protection" block.
                # I should parse that block in ProtectionEngine or here.
                # Let's update ProtectionEngine to load full settings or read main logic here.
                
                # Check whitelist
                if self.protection.is_whitelisted(info.get("name")) or \
                   self.protection.is_whitelisted(info.get("cmdline")): # Simple path check
                    action_taken = "Whitelisted"
                    threat_level = "Safe" # Override
                else:
                    # Check Auto Actions (Reading raw json or cached)
                    # We'll read the file quickly or better, cache it.
                    # reusing protection.load_settings logic for simplicity?
                    # ProtectionEngine.load_settings reads the whole file.
                    # I will add logic to read auto_suspend from file or assume defaults.
                    
                    with open(self.protection.config_file, 'r') as f:
                        settings_data = json.load(f)
                        prot_sets = settings_data.get("protection", {})
                        
                    if prot_sets.get("auto_suspend", False) and threat_level in ["medium", "high", "critical"]:
                        success, search_msg = self.protection.suspend_process(info["pid"])
                        if success:
                            action_taken = "Suspended"
                            self.stats["suspended"] += 1
                        else:
                            action_taken = f"Suspend Failed: {search_msg}"
                            
                    if prot_sets.get("auto_terminate", False) and threat_level in ["high", "critical"]:
                         # Override suspend if terminate is on
                        success, search_msg = self.protection.terminate_process(info["pid"])
                        if success:
                            action_taken = "Terminated"
                            self.stats["suspended"] += 1 # Count as stopped
                        else:
                            action_taken = f"Terminate Failed: {search_msg}"
            
            # Log
            self.logger.log_detection(info, threat_level)
            if action_taken not in ["Monitored", "Whitelisted"]:
                self.logger.log_action(info["pid"], action_taken, str(indicators))
                
        else:
            info["behavior"] = "Normal"
            info["threat_level"] = "Safe"

        info["action"] = action_taken
        
        # Update GUI
        self.window.panels["Live Monitoring"].add_process_row(info)

    def update_dashboard_tick(self):
        """ Independent loop to update stats like total process count """
        try:
            # Update Monitored Count (Total Running PIDs)
            # Since we monitor everything now, this is accurate.
            try:
                import psutil
                total_pids = len(psutil.pids())
                self.stats["monitored"] = total_pids
            except:
                pass
                
            self.window.panels["Dashboard"].update_stats(
                monitored=self.stats["monitored"],
                threats=self.stats["threats"],
                suspended=self.stats["suspended"],
                active=self.window.protection_active
            )
        except Exception:
            pass
        finally:
            self.window.after(1000, self.update_dashboard_tick)

    def run(self):
        self.update_dashboard_tick() # Start the loop
        self.window.mainloop()

if __name__ == "__main__":
    app = WatchTowerApp()
    app.run()
