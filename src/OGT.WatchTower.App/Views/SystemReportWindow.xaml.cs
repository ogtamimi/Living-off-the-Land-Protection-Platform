using System.Windows;
using System.Windows.Media;

namespace OGT.WatchTower.App.Views
{
    public partial class SystemReportWindow : Window
    {
        public SystemReportWindow(bool isSecure, string threatName, string severity, string location, string description, string actionTaken)
        {
            InitializeComponent();
            LoadData(isSecure, threatName, severity, location, description, actionTaken);
        }

        private void LoadData(bool isSecure, string threatName, string severity, string location, string description, string actionTaken)
        {
            // Override security status if threat was neutralized
            if (actionTaken.ToUpper().Contains("TERMINATED") || 
                actionTaken.ToUpper().Contains("KILLED") || 
                actionTaken.ToUpper().Contains("NEUTRALIZED") ||
                actionTaken.ToUpper().Contains("SUSPENDED"))
            {
                isSecure = true;
            }

            if (!isSecure)
            {
                StatusText.Text = "AT RISK";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(255, 68, 68)); // #FF4444
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(255, 68, 68));
            }
            else
            {
                 StatusText.Text = "SECURE";
                 StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 136)); // #00FF88
                 StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 136));
            }

            ThreatNameText.Text = threatName;
            SeverityText.Text = severity.ToUpper();
            LocationText.Text = location;
            DescriptionText.Text = description;
            ActionText.Text = actionTaken;

            // Color code severity
            if (severity.ToLower() == "critical") SeverityBadge.Background = new SolidColorBrush(Color.FromRgb(255, 68, 68));
            else if (severity.ToLower() == "high") SeverityBadge.Background = new SolidColorBrush(Color.FromRgb(255, 150, 0));
            else SeverityBadge.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
            
            // Color code action
            if (actionTaken.ToUpper().Contains("TERMINATED") || 
                actionTaken.ToUpper().Contains("SUSPENDED") || 
                actionTaken.ToUpper().Contains("NEUTRALIZED") ||
                actionTaken.ToUpper().Contains("KILLED")) 
                ActionText.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 136)); // #00FF88
            else if (actionTaken.ToUpper().Contains("FAILED"))
                 ActionText.Foreground = new SolidColorBrush(Color.FromRgb(255, 68, 68));
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}