using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using OGT.WatchTower.Core.Config;
using OGT.WatchTower.Core.Detection;
using OGT.WatchTower.Core.Logging;
using OGT.WatchTower.Core.Monitor;
using OGT.WatchTower.Core.Prevention;

namespace OGT.WatchTower.App
{
    public partial class MainWindow : Window
    {
        private SysmonMonitor? _monitor;
        private DetectionEngine? _engine;
        private bool _protectionActive = true;
        
        // Report Data
        private class ThreatInfo
        {
            public string Name { get; set; } = "None";
            public string Level { get; set; } = "-";
            public string Location { get; set; } = "-";
            public string Description { get; set; } = "No threats detected in the current session.";
            public string ActionTaken { get; set; } = "-";
            public bool IsSecure { get; set; } = true;
        }
        private ThreatInfo _lastThreat = new ThreatInfo();

        // Simple view caching
        private UserControl? _dashboardView;
        private UserControl? _monitoringView;
        private UserControl? _logsView;
        private UserControl? _settingsView;
        private UserControl? _forensicsView;
        private UserControl? _intelligenceView;

        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                this.Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Assets/icon.ico"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load icon: {ex.Message}");
            }
            
            // Initialize Core
            InitializeEngine();
            
            // Show Dashboard
            ShowDashboard();
        }

        private void InitializeEngine()
        {
            try 
            {
                Logger.Instance.Log("ACTION", "Starting WatchTower Engine", "Initializing components...");

                // Path to rules (relative to executable)
                string rulesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "rules");
                _engine = new DetectionEngine(rulesPath);
                Logger.Instance.Log("ACTION", "Detection Engine Initialized", $"Loaded rules from {rulesPath}");
                
                _monitor = new SysmonMonitor(OnEventDetected, OnError);
                _monitor.Start();
                Logger.Instance.Log("ACTION", "Sysmon Monitor Started", "Listening for events...");
            }
            catch (Exception ex)
            {
                Logger.Instance.Log("ERROR", "Engine Initialization Failed", ex.Message);
                MessageBox.Show($"Failed to start engine: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnEventDetected(Dictionary<string, object> eventData)
        {
            // Run Detection
            if (_protectionActive && _engine != null)
            {
                var matches = _engine.Evaluate(eventData);
                
                ThreatInfo? bestThreatInfo = null;
                bool eventProcessTerminated = false;

                foreach (var match in matches)
                {
                    // Log Detection
                    Logger.Instance.Log("DETECTION", match.Title, $"Level: {match.Level} | Event: {eventData.GetValueOrDefault("EventID", "?")}");

                    // Prepare Report Info
                    var threatInfo = new ThreatInfo
                    {
                        Name = match.Title,
                        Level = match.Level,
                        Description = match.Description ?? "Threat detected based on behavioral analysis.",
                        Location = eventData.GetValueOrDefault("Image")?.ToString() ?? eventData.GetValueOrDefault("CommandLine")?.ToString() ?? "Unknown",
                        ActionTaken = "Detected Only",
                        IsSecure = false
                    };

                    // Active Protection
                    if (_protectionActive)
                    {
                        if (eventData.TryGetValue("ProcessId", out var pidObj) && int.TryParse(pidObj?.ToString(), out int pid))
                        {
                            var settings = SettingsManager.Instance.Settings;
                            bool actionTaken = false;
                            string actionTitle = "THREAT NEUTRALIZED";

                            if (eventProcessTerminated)
                            {
                                // Process already killed by a previous rule in this batch
                                actionTaken = true;
                                actionTitle = "PROCESS TERMINATED";
                            }
                            else
                            {
                                // Check Auto-Kill (High/Critical OR matching specific attack patterns)
                                if (settings.Response.AutoKill && 
                                    (match.Level.Equals("high", StringComparison.OrdinalIgnoreCase) || 
                                     match.Level.Equals("critical", StringComparison.OrdinalIgnoreCase) ||
                                     match.Title.Contains("PowerShell Encoded")))
                                {
                                    if (ProcessManager.KillProcess(pid))
                                    {
                                        Logger.Instance.Log("PROTECTION", "Process Terminated", $"Killed process {pid} triggered by rule {match.Title}");
                                        actionTaken = true;
                                        actionTitle = "PROCESS TERMINATED";
                                        eventProcessTerminated = true;
                                    }
                                }
                                // Check Auto-Suspend (Fallback if enabled)
                                else if (settings.Response.AutoSuspend)
                                {
                                    if (ProcessManager.SuspendProcess(pid))
                                    {
                                        Logger.Instance.Log("PROTECTION", "Process Suspended", $"Suspended process {pid} triggered by rule {match.Title}");
                                        actionTaken = true;
                                        actionTitle = "PROCESS SUSPENDED";
                                    }
                                }
                            }

                            if (actionTaken)
                            {
                                threatInfo.ActionTaken = actionTitle;
                                threatInfo.IsSecure = true;
                                Dispatcher.Invoke(() => AddAlert(actionTitle, "Critical", eventData));
                            }
                        }
                    }
                    
                    Dispatcher.Invoke(() => {
                        AddAlert(match.Title, match.Level, eventData);
                    });

                    // Update Best Threat Info (Prioritize Secure/Action Taken)
                    if (bestThreatInfo == null)
                    {
                        bestThreatInfo = threatInfo;
                    }
                    else if (threatInfo.IsSecure && !bestThreatInfo.IsSecure)
                    {
                        bestThreatInfo = threatInfo;
                    }
                    else if (threatInfo.IsSecure == bestThreatInfo.IsSecure)
                    {
                        // Tie-breaker: Priority to High/Critical severity
                        if ((match.Level.ToLower() == "critical" || match.Level.ToLower() == "high") && 
                            (bestThreatInfo.Level.ToLower() != "critical" && bestThreatInfo.Level.ToLower() != "high"))
                        {
                            bestThreatInfo = threatInfo;
                        }
                    }
                }

                // Update Last Threat with the most significant one from this batch
                if (bestThreatInfo != null)
                {
                    _lastThreat = bestThreatInfo;
                }
            }

            // Update Live Feed
            Dispatcher.Invoke(() => {
                if (_monitoringView is Views.MonitoringView mon)
                {
                    mon.AddEvent(eventData);
                }
            });
        }
        
        private void AddAlert(string title, string level, Dictionary<string, object> data)
        {
            // Extract PID if available
            int pid = 0;
            if (data.TryGetValue("ProcessId", out var pidObj) && int.TryParse(pidObj?.ToString(), out int parsedPid))
            {
                pid = parsedPid;
            }

            // Add to Dashboard Alerts
            if (_dashboardView is Views.DashboardView dash)
            {
                dash.AddAlert(title, level, DateTime.Now.ToString("HH:mm:ss"), pid);
            }
        }

        private void OnError(string error)
        {
            // Log error
            Logger.Instance.Log("ERROR", "Sysmon Monitor Error", error);
            System.Diagnostics.Debug.WriteLine(error);
        }

        private void Nav_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
        }

        private void Nav_Monitoring_Click(object sender, RoutedEventArgs e)
        {
            if (_monitoringView == null) _monitoringView = new Views.MonitoringView();
            MainContent.Content = _monitoringView;
            PageTitle.Text = "Live Monitoring";
        }

        private void Nav_Logs_Click(object sender, RoutedEventArgs e)
        {
            if (_logsView == null) _logsView = new Views.LogsView();
            MainContent.Content = _logsView;
            PageTitle.Text = "System Logs";
        }

        private void Nav_Forensics_Click(object sender, RoutedEventArgs e)
        {
            if (_forensicsView == null) _forensicsView = new Views.ForensicsView();
            MainContent.Content = _forensicsView;
            PageTitle.Text = "Forensics & Analysis";
        }

        private void Nav_Intelligence_Click(object sender, RoutedEventArgs e)
        {
            if (_intelligenceView == null) _intelligenceView = new Views.IntelligenceView();
            MainContent.Content = _intelligenceView;
            PageTitle.Text = "Threat Intelligence";
        }

        private void Nav_Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsView == null) _settingsView = new Views.SettingsView();
            MainContent.Content = _settingsView;
            PageTitle.Text = "Configuration";
        }

        private void ShowDashboard()
        {
            if (_dashboardView == null) _dashboardView = new Views.DashboardView();
            MainContent.Content = _dashboardView;
            PageTitle.Text = "Dashboard";
        }

        private void Protection_Toggle_Click(object sender, RoutedEventArgs e)
        {
            _protectionActive = !_protectionActive;
            
            // Find the Ellipse inside the button template to update it
            var statusDot = ProtectionBtn.Template.FindName("statusDot", ProtectionBtn) as System.Windows.Shapes.Ellipse;

            if (_protectionActive)
            {
                ProtectionBtn.Content = "ACTIVE";
                ProtectionBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FF88"));
                if (statusDot != null) statusDot.Fill = System.Windows.Media.Brushes.Black;
                ProtectionBtn.Foreground = System.Windows.Media.Brushes.Black;
            }
            else
            {
                ProtectionBtn.Content = "PAUSED";
                ProtectionBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4444"));
                if (statusDot != null) statusDot.Fill = System.Windows.Media.Brushes.White;
                ProtectionBtn.Foreground = System.Windows.Media.Brushes.White;
            }
        }
        
        private void Report_Click(object sender, RoutedEventArgs e)
        {
            var info = _lastThreat;
            var reportWindow = new Views.SystemReportWindow(info.IsSecure, info.Name, info.Level, info.Location, info.Description, info.ActionTaken);
            reportWindow.Owner = this;
            reportWindow.ShowDialog();
        }

        protected override void OnClosed(EventArgs e)
        {
            _monitor?.Stop();
            base.OnClosed(e);
        }
    }
}
