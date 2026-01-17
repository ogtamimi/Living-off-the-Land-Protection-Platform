import customtkinter as ctk
from tkinter import ttk
import tkinter as tk
from datetime import datetime
import json
import os

class LogsPanel(ctk.CTkFrame):
    def __init__(self, parent, logger_instance=None):
        super().__init__(parent, fg_color="transparent")
        
        self.logger = logger_instance
        self.grid_columnconfigure(0, weight=1)
        self.grid_rowconfigure(1, weight=1)
        
        # Controls
        controls_frame = ctk.CTkFrame(self, fg_color="transparent")
        controls_frame.grid(row=0, column=0, sticky="ew", padx=0, pady=(0, 10))
        
        self.filter_var = ctk.StringVar(value="All")
        filter_combo = ctk.CTkComboBox(controls_frame, values=["All", "DETECTION", "ACTION", "ERROR"], 
                                       command=self.filter_logs, variable=self.filter_var)
        filter_combo.pack(side="left", padx=(0, 10))
        
        refresh_btn = ctk.CTkButton(controls_frame, text="Refresh", width=100, command=self.load_logs,
                                    fg_color="#00d4ff", text_color="black")
        refresh_btn.pack(side="left")
        
        export_btn = ctk.CTkButton(controls_frame, text="Export CSV", width=100, command=self.export_logs,
                                   fg_color="#2d2d2d", border_width=1, border_color="#555555")
        export_btn.pack(side="right")

        # Table
        style = ttk.Style()
        style.configure("Treeview.Heading", font=('Arial', 10, 'bold'))
        
        self.columns = ("Timestamp", "Type", "Details", "PID")
        self.tree = ttk.Treeview(self, columns=self.columns, show="headings", selectmode="browse")
        
        for col in self.columns:
            self.tree.heading(col, text=col)
            
        self.tree.column("Timestamp", width=150)
        self.tree.column("Type", width=100)
        self.tree.column("Details", width=400)
        self.tree.column("PID", width=80)
        
        # Scrollbar
        scrollbar = ctk.CTkScrollbar(self, command=self.tree.yview)
        self.tree.configure(yscrollcommand=scrollbar.set)
        
        self.tree.grid(row=1, column=0, sticky="nsew")
        scrollbar.grid(row=1, column=1, sticky="ns")
        
        self.full_logs = []
        if self.logger:
             self.load_logs()

    def set_logger(self, logger):
        self.logger = logger
        self.load_logs()

    def load_logs(self):
        # Clear current
        for item in self.tree.get_children():
            self.tree.delete(item)
            
        if not self.logger:
            return

        self.full_logs = self.logger.get_all_logs() or []
        # Sort by timestamp desc
        self.full_logs.sort(key=lambda x: x.get("timestamp", ""), reverse=True)
        
        self.filter_logs()

    def filter_logs(self, value=None):
        filter_type = self.filter_var.get()
        
        # Clear view
        for item in self.tree.get_children():
            self.tree.delete(item)
            
        for log in self.full_logs:
            if filter_type != "All" and log.get("type") != filter_type:
                continue
                
            # Flatten details
            details = ""
            if "process_info" in log:
                 di = log["process_info"]
                 details = f"{di.get('name')} - {di.get('cmdline')[:50]}..."
            elif "action" in log:
                 details = f"{log.get('action')} - {log.get('details')}"
            else:
                 details = str(log)
            
            values = (
                log.get("timestamp", "").replace("T", " "),
                log.get("type", ""),
                details,
                str(log.get("pid", ""))
            )
            self.tree.insert("", "end", values=values)

    def export_logs(self):
        if self.logger:
            filename = f"exported_logs_{datetime.now().strftime('%Y%m%d_%H%M%S')}.csv"
            path = os.path.join(self.logger.log_dir, filename)
            if self.logger.export_logs(path, format="csv"):
                print(f"Exported to {path}") # Ideally show a dialog
