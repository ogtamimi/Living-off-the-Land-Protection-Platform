using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Xml.Linq;
using System.Management;
using OGT.WatchTower.Core.Logging;

namespace OGT.WatchTower.Core.Monitor
{
    public class SysmonMonitor
    {
        private EventLogWatcher? _watcher;
        private ManagementEventWatcher? _wmiWatcher;
        private Action<Dictionary<string, object>> _eventCallback;
        private Action<string> _errorCallback;

        public SysmonMonitor(Action<Dictionary<string, object>> eventCallback, Action<string> errorCallback)
        {
            _eventCallback = eventCallback;
            _errorCallback = errorCallback;
        }

        public void Start()
        {
            try
            {
                // Try Sysmon first
                var query = new EventLogQuery("Microsoft-Windows-Sysmon/Operational", PathType.LogName, "*[System[(EventID=1)]]");
                _watcher = new EventLogWatcher(query);
                _watcher.EventRecordWritten += Watcher_EventRecordWritten;
                _watcher.Enabled = true;
            }
            catch (Exception)
            {
                // Sysmon failed, try WMI Fallback
                try
                {
                    StartWMI();
                }
                catch (Exception wmiEx)
                {
                    _errorCallback?.Invoke($"Failed to start monitoring. Sysmon not found and WMI failed: {wmiEx.Message}");
                }
            }
        }

        private void StartWMI()
        {
            try 
            {
                var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
                _wmiWatcher = new ManagementEventWatcher(query);
                _wmiWatcher.EventArrived += WmiWatcher_EventArrived;
                _wmiWatcher.Start();
            }
            catch (Exception ex)
            {
                throw new Exception($"WMI Error: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.Enabled = false;
                _watcher.Dispose();
                _watcher = null;
            }

            if (_wmiWatcher != null)
            {
                _wmiWatcher.Stop();
                _wmiWatcher.Dispose();
                _wmiWatcher = null;
            }
        }

        private void WmiWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var dict = new Dictionary<string, object>();
                dict["EventID"] = "1"; // Treat as Process Create
                
                string processName = e.NewEvent.Properties["ProcessName"].Value?.ToString() ?? "Unknown";
                string processIdStr = e.NewEvent.Properties["ProcessID"].Value?.ToString() ?? "0";
                
                dict["Image"] = processName;
                dict["ProcessId"] = processIdStr;

                // Fetch details from Win32_Process
                if (int.TryParse(processIdStr, out int pid) && pid > 0)
                {
                    var details = GetProcessDetails(pid);
                    dict["CommandLine"] = !string.IsNullOrEmpty(details.CommandLine) ? details.CommandLine : processName;
                    dict["User"] = !string.IsNullOrEmpty(details.User) ? details.User : "System";
                    
                    // If Image is just filename, try to get full path
                    if (!processName.Contains("\\") && !string.IsNullOrEmpty(details.ExecutablePath))
                    {
                        dict["Image"] = details.ExecutablePath;
                    }
                }
                else
                {
                    dict["User"] = "System";
                    dict["CommandLine"] = processName;
                }

                // Debug Log
                Logger.Instance.Log("DEBUG", "WMI Event Received", $"Process: {dict["Image"]} ({dict["ProcessId"]}) CMD: {dict["CommandLine"]}");

                _eventCallback?.Invoke(dict);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log("ERROR", "WMI Event Parsing Error", ex.Message);
            }
        }

        private (string CommandLine, string User, string ExecutablePath) GetProcessDetails(int pid)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT CommandLine, ExecutablePath FROM Win32_Process WHERE ProcessId = {pid}"))
                using (var objects = searcher.Get())
                {
                    foreach (ManagementObject obj in objects)
                    {
                        string cmd = obj["CommandLine"]?.ToString() ?? "";
                        string path = obj["ExecutablePath"]?.ToString() ?? "";
                        string user = "";

                        try 
                        {
                            string[] ownerInfo = new string[2];
                            obj.InvokeMethod("GetOwner", ownerInfo);
                            if (ownerInfo[0] != null)
                            {
                                user = $"{ownerInfo[1]}\\{ownerInfo[0]}";
                            }
                        }
                        catch {}

                        return (cmd, user, path);
                    }
                }
            }
            catch (Exception ex)
            {
                 Logger.Instance.Log("WARN", "WMI Detail Fetch Failed", ex.Message);
            }
            return ("", "", "");
        }

        private void Watcher_EventRecordWritten(object sender, EventRecordWrittenEventArgs e)
        {
            if (e.EventRecord == null) return;

            try
            {
                var dict = ParseEvent(e.EventRecord);
                _eventCallback?.Invoke(dict);
            }
            catch (Exception ex)
            {
                _errorCallback?.Invoke($"Error parsing event: {ex.Message}");
            }
        }

        private Dictionary<string, object> ParseEvent(EventRecord record)
        {
            var data = new Dictionary<string, object>();
            
            // Standard Event Data
            data["EventID"] = record.Id;
            data["TimeCreated"] = record.TimeCreated;
            
            // Parse XML Data
            var xml = record.ToXml();
            var doc = XDocument.Parse(xml);
            var ns = doc.Root.Name.Namespace;
            
            var eventData = doc.Root.Element(ns + "EventData");
            if (eventData != null)
            {
                foreach (var dataPoint in eventData.Elements(ns + "Data"))
                {
                    var name = dataPoint.Attribute("Name")?.Value;
                    var value = dataPoint.Value;
                    
                    if (!string.IsNullOrEmpty(name))
                    {
                        data[name] = value;
                    }
                }
            }
            
            // Normalize keys for Sigma (e.g., Image -> Image, CommandLine -> CommandLine)
            // Sigma rules usually look for "Image", "CommandLine", "ParentImage", "User", etc.
            // Sysmon provides these exact names in Data.
            
            return data;
        }
    }
}
