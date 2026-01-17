import customtkinter as ctk
from .dashboard import Dashboard
from .monitoring_panel import MonitoringPanel
from .logs_panel import LogsPanel
from .settings_panel import SettingsPanel

class MainWindow(ctk.CTk):
    def __init__(self):
        super().__init__()
        
        # Window Setup
        self.title("OGT WatchTower - LOtL Binaries Watcher")
        self.geometry("1100x700")
        
        # Configuration
        ctk.set_appearance_mode("Dark")
        ctk.set_default_color_theme("blue") # Uses blue accent, close to cyan
        
        # Grid Layout
        self.grid_columnconfigure(1, weight=1)
        self.grid_rowconfigure(1, weight=1)
        
        # -- Header --
        self.header = ctk.CTkFrame(self, height=60, fg_color="#1a1a1a", corner_radius=0)
        self.header.grid(row=0, column=0, columnspan=2, sticky="ew")
        self.setup_header()
        
        # -- Sidebar --
        self.sidebar = ctk.CTkFrame(self, width=200, fg_color="#2d2d2d", corner_radius=0)
        self.sidebar.grid(row=1, column=0, sticky="ns")
        self.setup_sidebar()
        
        # -- Main Area --
        self.main_area = ctk.CTkFrame(self, fg_color="transparent")
        self.main_area.grid(row=1, column=1, sticky="nsew", padx=20, pady=20)
        
        # Initialize Panels
        self.current_panel = None
        self.panels = {}
        
        self.panels["Dashboard"] = Dashboard(self.main_area)
        self.panels["Live Monitoring"] = MonitoringPanel(self.main_area)
        self.panels["Logs"] = LogsPanel(self.main_area)
        self.panels["Settings"] = SettingsPanel(self.main_area)
        
        # Show default
        self.show_panel("Dashboard")

    def setup_header(self):
        # Logo/Title
        title = ctk.CTkLabel(self.header, text="OGT WatchTower", font=("Arial", 20, "bold"), text_color="#00d4ff")
        title.pack(side="left", padx=20, pady=10)
        
        # Protection Status (Right side)
        self.protection_btn = ctk.CTkButton(self.header, text="Protection: ACTIVE", 
                                            fg_color="#00ff88", text_color="black", hover_color="#00cc66",
                                            command=self.toggle_protection)
        self.protection_btn.pack(side="right", padx=20, pady=10)
        
        self.protection_active = True

    def setup_sidebar(self):
        # Navigation Buttons
        buttons = ["Dashboard", "Live Monitoring", "Logs", "Settings"]
        
        for btn_text in buttons:
            btn = ctk.CTkButton(self.sidebar, text=btn_text, 
                                fg_color="transparent", hover_color="#444444", 
                                anchor="w", height=40,
                                command=lambda t=btn_text: self.show_panel(t))
            btn.pack(fill="x", padx=10, pady=5)

    def show_panel(self, panel_name):
        if self.current_panel:
            self.current_panel.pack_forget()
            
        if panel_name in self.panels:
            self.current_panel = self.panels[panel_name]
            self.current_panel.pack(fill="both", expand=True)

    def toggle_protection(self):
        self.protection_active = not self.protection_active
        if self.protection_active:
            self.protection_btn.configure(text="Protection: ACTIVE", fg_color="#00ff88")
        else:
            self.protection_btn.configure(text="Protection: PAUSED", fg_color="#ff4444")
            
        # Hook for external controller to observe this change
        if hasattr(self, "on_protection_toggle"):
            self.on_protection_toggle(self.protection_active)
