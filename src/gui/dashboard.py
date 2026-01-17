import customtkinter as ctk

class Dashboard(ctk.CTkFrame):
    def __init__(self, parent):
        super().__init__(parent, fg_color="transparent")
        
        self.grid_columnconfigure((0, 1, 2, 3), weight=1)
        self.grid_rowconfigure(0, weight=1)
        
        # Stats
        self.monitored_card = self.create_stat_card(0, "Monitored Processes", "0", "gray")
        self.threats_card = self.create_stat_card(1, "Threats Detected", "0", "#ff4444")
        self.suspended_card = self.create_stat_card(2, "Suspended", "0", "#ffaa00")
        self.uptime_card = self.create_stat_card(3, "System Status", "Active", "#00ff88")
        
        self.stats = {
            "monitored": 0,
            "threats": 0,
            "suspended": 0
        }

    def create_stat_card(self, col, title, value, color_indicator):
        # Increased corner radius and "hollow" look or just bigger
        card = ctk.CTkFrame(self, fg_color="#2d2d2d", corner_radius=15, height=140) # Explicit height preference
        card.grid(row=0, column=col, padx=10, pady=20, sticky="nsew") # sticky nsew to fill grid cell
        
        # Using a slightly responsive font size or just careful selection
        # "MONITORED PROCESSES" is long. Splitting to two lines or smaller font?
        # User wants "text bigger". 
        # Title
        title_label = ctk.CTkLabel(card, text=title.upper(), font=("Roboto", 14, "bold"), text_color="#aaaaaa")
        title_label.pack(pady=(25, 5), padx=15, anchor="w")
        
        # Value - BIG
        value_label = ctk.CTkLabel(card, text=value, font=("Roboto", 48, "bold"), text_color="white")
        value_label.pack(pady=(0, 25), padx=15, anchor="w")
        
        # Strip indicator
        indicator = ctk.CTkFrame(card, height=8, fg_color=color_indicator, corner_radius=4)
        indicator.pack(fill="x", side="bottom", padx=0, pady=0)
        
        return value_label

    def update_stats(self, monitored=None, threats=None, suspended=None, active=True):
        if monitored is not None:
            self.monitored_card.configure(text=str(monitored))
        if threats is not None:
            self.threats_card.configure(text=str(threats))
        if suspended is not None:
            self.suspended_card.configure(text=str(suspended))
            
        status_text = "Active" if active else "Inactive"
        status_color = "#00ff88" if active else "#ff4444"
        self.uptime_card.configure(text=status_text, text_color=status_color)
