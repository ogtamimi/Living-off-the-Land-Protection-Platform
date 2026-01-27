using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OGT.WatchTower.App.Views
{
    public partial class ForensicsView : UserControl
    {
        private DispatcherTimer _timer;
        private Dictionary<int, (TimeSpan TotalProcessorTime, DateTime SampleTime)> _prevCpuInfo = new Dictionary<int, (TimeSpan, DateTime)>();
        private object _cpuLock = new object();

        public ForensicsView()
        {
            InitializeComponent();
            RefreshProcesses();

            // Auto-refresh every 3 seconds for better CPU stats
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += (s, e) => RefreshProcesses();
            _timer.Start();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcesses();
        }

        private void RefreshProcesses()
        {
            // Run on background thread to avoid UI freeze
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var processList = new List<ProcessItem>();
                    var processes = Process.GetProcesses();
                    long totalThreads = 0;

                    foreach (var p in processes)
                    {
                        try
                        {
                            var item = new ProcessItem
                            {
                                Id = p.Id,
                                Name = p.ProcessName,
                                MemoryMB = p.WorkingSet64 / 1024 / 1024,
                                HandleCount = p.HandleCount
                            };

                            try { item.ThreadCount = p.Threads.Count; } catch { item.ThreadCount = 0; }
                            totalThreads += item.ThreadCount;

                            // Calculate CPU Usage
                            try
                            {
                                var now = DateTime.UtcNow;
                                var currentTotalProcessorTime = p.TotalProcessorTime;
                                
                                lock (_cpuLock)
                                {
                                    if (_prevCpuInfo.TryGetValue(p.Id, out var prev))
                                    {
                                        var cpuUsedMs = (currentTotalProcessorTime - prev.TotalProcessorTime).TotalMilliseconds;
                                        var totalMs = (now - prev.SampleTime).TotalMilliseconds;
                                        if (totalMs > 0)
                                        {
                                            var cpuUsage = (cpuUsedMs / (totalMs * Environment.ProcessorCount)) * 100;
                                            item.CpuPercent = Math.Round(cpuUsage, 1);
                                        }
                                    }
                                    
                                    _prevCpuInfo[p.Id] = (currentTotalProcessorTime, now);
                                }
                            }
                            catch { /* Access denied to processor time */ }

                            // Try to get StartTime (may be denied)
                            try { item.StartTime = p.StartTime.ToString("HH:mm:ss"); } catch { item.StartTime = "-"; }
                            
                            // Try to get Path (may be denied)
                            try { item.Path = p.MainModule?.FileName ?? "-"; } catch { item.Path = "-"; }

                            // Simple heuristics for status
                            if (item.Name.Contains("powershell") || item.Name.Contains("cmd"))
                                item.Status = "Suspicious";
                            else if (item.MemoryMB > 500)
                                item.Status = "High Mem";
                            else
                                item.Status = "Normal";

                            processList.Add(item);
                        }
                        catch
                        {
                            // Skip processes we can't access
                        }
                        finally
                        {
                            p.Dispose();
                        }
                    }

                    var sorted = processList.OrderByDescending(p => p.MemoryMB).ToList();

                    // Cleanup dead processes from CPU cache
                    var currentIds = processList.Select(p => p.Id).ToHashSet();
                    
                    lock (_cpuLock)
                    {
                        var deadIds = _prevCpuInfo.Keys.Where(k => !currentIds.Contains(k)).ToList();
                        foreach (var id in deadIds) _prevCpuInfo.Remove(id);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ProcessList.ItemsSource = sorted;
                        LblCount.Text = $"{sorted.Count} Processes Active";
                        LblThreads.Text = $"{totalThreads:N0} Threads Detected";
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in RefreshProcesses: {ex.Message}");
                }
            });
        }

        public class ProcessItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double CpuPercent { get; set; }
        public long MemoryMB { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public string StartTime { get; set; } = "";
        public string Path { get; set; } = "";
        public string Status { get; set; } = "Normal";
        
        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Suspicious" => "#FF4444", // Red
                    "High Mem" => "#FFAA00",    // Orange
                    _ => "#00FF88"              // Green
                };
            }
        }
    }
}
}
