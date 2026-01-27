using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using OGT.WatchTower.Core.Prevention;

namespace OGT.WatchTower.App.Views
{
    public partial class DashboardView : UserControl
    {
        public ObservableCollection<AlertItem> Alerts { get; set; } = new ObservableCollection<AlertItem>();
        private DispatcherTimer _timer;

        public DashboardView()
        {
            InitializeComponent();
            AlertsList.ItemsSource = Alerts;
            ActiveRulesText.Text = "6"; // Hardcoded for demo, normally read from engine

            // Start process counter timer
            UpdateProcessCount();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += (s, e) => UpdateProcessCount();
            _timer.Start();
        }

        private void UpdateProcessCount()
        {
            try
            {
                int count = Process.GetProcesses().Length;
                ActiveProcessesText.Text = count.ToString();
            }
            catch { }
        }

        public void AddAlert(string title, string level, string time, int pid = 0)
        {
            Alerts.Insert(0, new AlertItem { Title = title, Level = level, Time = time, ProcessId = pid });
            AlertsCountText.Text = Alerts.Count.ToString();
        }

        private void KillButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AlertItem alert)
            {
                if (alert.ProcessId > 0)
                {
                    bool success = ProcessManager.KillProcess(alert.ProcessId);
                    if (success)
                    {
                        MessageBox.Show($"Successfully killed process {alert.ProcessId}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to kill process {alert.ProcessId}. It may have already exited.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No active Process ID associated with this alert.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }

    public class AlertItem
    {
        public required string Time { get; set; }
        public required string Level { get; set; }
        public required string Title { get; set; }
        public int ProcessId { get; set; }
        
        // Helper to show/hide button if PID is valid
        public Visibility ButtonVisibility => ProcessId > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}
