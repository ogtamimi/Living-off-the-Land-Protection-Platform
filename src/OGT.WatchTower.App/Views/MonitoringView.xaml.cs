using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace OGT.WatchTower.App.Views
{
    public partial class MonitoringView : UserControl
    {
        public ObservableCollection<ProcessEvent> Events { get; set; } = new ObservableCollection<ProcessEvent>();

        public MonitoringView()
        {
            InitializeComponent();
            EventsGrid.ItemsSource = Events;
            
            // Add a test event to verify UI is working
            Events.Add(new ProcessEvent 
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                EventId = "TEST",
                Image = "MonitoringView Loaded",
                User = "System",
                CommandLine = "Verifying UI Binding..."
            });
        }

        public void AddEvent(Dictionary<string, object> data)
        {
            var evt = new ProcessEvent
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                EventId = data.ContainsKey("EventID") ? (data["EventID"]?.ToString() ?? "?") : "?",
                Image = data.ContainsKey("Image") ? (data["Image"]?.ToString() ?? "") : "",
                User = data.ContainsKey("User") ? (data["User"]?.ToString() ?? "") : "",
                CommandLine = data.ContainsKey("CommandLine") ? (data["CommandLine"]?.ToString() ?? "") : ""
            };

            // Keep only last 1000 events
            if (Events.Count > 1000) Events.RemoveAt(0);
            
            Events.Add(evt);
            
            // Auto scroll
            if (Events.Count > 0)
                EventsGrid.ScrollIntoView(evt);
        }
    }

    public class ProcessEvent
    {
        public required string Time { get; set; }
        public required string EventId { get; set; }
        public required string Image { get; set; }
        public required string User { get; set; }
        public required string CommandLine { get; set; }
    }
}
