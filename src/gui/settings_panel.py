import customtkinter as ctk
import json
import os

from core.utils import get_config_path

class SettingsPanel(ctk.CTkFrame):
    def __init__(self, parent):
        super().__init__(parent, fg_color="transparent")
        
        self.settings_file = get_config_path("settings.json")
        self.lolbins_file = get_config_path("lolbins.json")
        
        self.grid_columnconfigure(0, weight=1)
        
        # --- Protection Settings ---
        self.protection_frame = ctk.CTkFrame(self, fg_color="#2d2d2d")
        self.protection_frame.grid(row=0, column=0, sticky="ew", padx=10, pady=10)
        
        ctk.CTkLabel(self.protection_frame, text="Protection Settings", font=("Arial", 16, "bold")).pack(anchor="w", padx=10, pady=10)
        
        self.auto_suspend_var = ctk.BooleanVar()
        self.auto_terminate_var = ctk.BooleanVar()
        self.show_alerts_var = ctk.BooleanVar()
        
        ctk.CTkCheckBox(self.protection_frame, text="Auto Suspend Suspicious Processes", variable=self.auto_suspend_var).pack(anchor="w", padx=10, pady=5)
        ctk.CTkCheckBox(self.protection_frame, text="Auto Terminate High Risk (Dangerous)", variable=self.auto_terminate_var).pack(anchor="w", padx=10, pady=5)
        ctk.CTkCheckBox(self.protection_frame, text="Show Desktop Alerts", variable=self.show_alerts_var).pack(anchor="w", padx=10, pady=5)
        
        # --- Monitored Tools ---
        self.tools_frame = ctk.CTkScrollableFrame(self, fg_color="#2d2d2d", label_text="Monitored Tools", label_font=("Arial", 16, "bold"), height=200)
        self.tools_frame.grid(row=1, column=0, sticky="ew", padx=10, pady=10)
        
        self.tool_vars = {} 
        # Loaded dynamically in load_settings
        
        # --- Whitelist ---
        self.whitelist_frame = ctk.CTkFrame(self, fg_color="#2d2d2d")
        self.whitelist_frame.grid(row=2, column=0, sticky="ew", padx=10, pady=10)
        
        ctk.CTkLabel(self.whitelist_frame, text="Whitelist (Full Paths)", font=("Arial", 16, "bold")).pack(anchor="w", padx=10, pady=10)
        
        self.whitelist_entry = ctk.CTkEntry(self.whitelist_frame, placeholder_text="C:\\Path\\To\\File.exe", width=300)
        self.whitelist_entry.pack(side="left", padx=10, pady=10)
        
        ctk.CTkButton(self.whitelist_frame, text="Add", width=60, command=self.add_whitelist).pack(side="left", padx=0, pady=10)
        
        self.whitelist_text = ctk.CTkTextbox(self.whitelist_frame, height=80)
        self.whitelist_text.pack(fill="x", padx=10, pady=10)

        # Save Button
        ctk.CTkButton(self, text="Save Settings", command=self.save_settings, fg_color="#00d4ff", text_color="black").grid(row=3, column=0, pady=20)
        
        self.load_settings()

    def load_settings(self):
        # Load Settings.json
        if os.path.exists(self.settings_file):
            try:
                with open(self.settings_file, 'r') as f:
                    settings = json.load(f)
                    prot = settings.get("protection", {})
                    notif = settings.get("notifications", {})
                    
                    self.auto_suspend_var.set(prot.get("auto_suspend", False))
                    self.auto_terminate_var.set(prot.get("auto_terminate", False))
                    self.show_alerts_var.set(notif.get("show_alerts", True))
                    
                    wl = settings.get("whitelist", [])
                    self.whitelist_text.delete("1.0", "end")
                    self.whitelist_text.insert("1.0", "\n".join(wl))
            except:
                pass
                
        # Load LOLBins
        if os.path.exists(self.lolbins_file):
            try:
                for widget in self.tools_frame.winfo_children():
                    widget.destroy()
                self.tool_vars = {}
                
                with open(self.lolbins_file, 'r') as f:
                    data = json.load(f)
                    for tool in data.get("monitored_tools", []):
                        name = tool["name"]
                        var = ctk.BooleanVar(value=tool.get("enabled", True))
                        self.tool_vars[name] = var
                        
                        chk = ctk.CTkCheckBox(self.tools_frame, text=name, variable=var)
                        chk.pack(anchor="w", padx=10, pady=2)
            except:
                pass

    def save_settings(self):
        # Save Settings.json
        try:
            settings = {
                "protection": {
                    "enabled": True, # Global toggle handled elsewhere usually
                    "auto_suspend": self.auto_suspend_var.get(),
                    "auto_terminate": self.auto_terminate_var.get(),
                    "sensitivity": "medium"
                },
                "notifications": {
                    "show_alerts": self.show_alerts_var.get(),
                    "sound_enabled": True
                },
                "whitelist": self.whitelist_text.get("1.0", "end").strip().split("\n")
            }
            # Filter empty strings in whitelist
            settings["whitelist"] = [x.strip() for x in settings["whitelist"] if x.strip()]
            
            with open(self.settings_file, 'w') as f:
                json.dump(settings, f, indent=4)
                
            # Save LOLBins
            with open(self.lolbins_file, 'r') as f:
                data = json.load(f)
                
            for tool in data.get("monitored_tools", []):
                name = tool["name"]
                if name in self.tool_vars:
                    tool["enabled"] = self.tool_vars[name].get()
                    
            with open(self.lolbins_file, 'w') as f:
                json.dump(data, f, indent=4)
                
            print("Settings saved.")
        except Exception as e:
            print(f"Error saving settings: {e}")

    def add_whitelist(self):
        path = self.whitelist_entry.get().strip()
        if path:
            current = self.whitelist_text.get("1.0", "end").strip()
            if current:
                self.whitelist_text.insert("end", f"\n{path}")
            else:
                self.whitelist_text.insert("1.0", path)
            self.whitelist_entry.delete(0, "end")
