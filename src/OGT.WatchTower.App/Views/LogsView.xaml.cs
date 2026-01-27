using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OGT.WatchTower.Core.Logging;

namespace OGT.WatchTower.App.Views
{
    public partial class LogsView : UserControl
    {
        private List<LogEntry>? _allLogs;

        public LogsView()
        {
            InitializeComponent();
            LoadLogs();
        }

        private void LoadLogs()
        {
            _allLogs = Logger.Instance.GetLogs();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allLogs == null) return;

            var filter = (FilterCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (string.IsNullOrEmpty(filter) || filter == "All")
            {
                LogsGrid.ItemsSource = _allLogs;
            }
            else
            {
                LogsGrid.ItemsSource = _allLogs.Where(l => l.Type == filter).ToList();
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }
    }
}
