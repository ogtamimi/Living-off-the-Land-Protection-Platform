import customtkinter as ctk
from tkinter import ttk
import tkinter as tk
from datetime import datetime

class MonitoringPanel(ctk.CTkFrame):
    def __init__(self, parent):
        super().__init__(parent, fg_color="transparent")
        
        self.grid_columnconfigure(0, weight=1)
        self.grid_rowconfigure(0, weight=1)
        
        # Style for Treeview
        style = ttk.Style()
        style.theme_use("default")
        
        # Configure colors for dark mode
        style.configure("Treeview", 
                        background="#2d2d2d", 
                        foreground="white", 
                        fieldbackground="#2d2d2d",
                        borderwidth=0,
                        rowheight=60, # Taller rows for modern look
                        font=("Roboto", 14)) # Bigger, readable font
        
        style.map('Treeview', background=[('selected', '#00d4ff')])
        
        style.configure("Treeview.Heading", 
                        background="#1a1a1a", 
                        foreground="white", 
                        font=('Roboto', 16, 'bold'), # Bold large headers
                        borderwidth=0)
        
        # Table
        self.columns = ("Timestamp", "Process", "PID", "Behavior", "Threat Level", "Action")
        self.tree = ttk.Treeview(self, columns=self.columns, show="headings", selectmode="browse")
        
        for col in self.columns:
            self.tree.heading(col, text=col)
            self.tree.column(col, width=100)
            
        self.tree.column("Behavior", width=300)
        self.tree.column("Timestamp", width=150)
        
        # Scrollbar
        scrollbar = ctk.CTkScrollbar(self, command=self.tree.yview)
        self.tree.configure(yscrollcommand=scrollbar.set)
        
        self.tree.grid(row=0, column=0, sticky="nsew", padx=(0, 5))
        scrollbar.grid(row=0, column=1, sticky="ns")
        
        # Tags for coloring
        self.tree.tag_configure("high", background="#4a2020", foreground="#ffaaaa")
        self.tree.tag_configure("medium", background="#4a3a20", foreground="#ffddaa")
        self.tree.tag_configure("safe", background="#2d2d2d", foreground="white")

    def add_process_row(self, process_data):
        """
        process_data: dict with timestamp, name, pid, behavior, threat_level, action
        """
        threat_level = process_data.get("threat_level", "low").lower()
        tag = "safe"
        if threat_level == "high" or threat_level == "critical":
            tag = "high"
        elif threat_level == "medium":
            tag = "medium"
            
        timestamp = process_data.get("timestamp")
        if isinstance(timestamp, float):
            timestamp = datetime.fromtimestamp(timestamp).strftime('%H:%M:%S')
            
        values = (
            timestamp,
            process_data.get("name", "Unknown"),
            process_data.get("pid", ""),
            process_data.get("behavior", ""),
            process_data.get("threat_level", "Low"),
            process_data.get("action", "Monitored")
        )
        
        # Insert at top
        self.tree.insert("", 0, values=values, tags=(tag,))
        
        # Limit rows (keep last 1000)
        if len(self.tree.get_children()) > 1000:
            self.tree.delete(self.tree.get_children()[-1])

    def clear(self):
        for item in self.tree.get_children():
            self.tree.delete(item)
