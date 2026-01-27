using System.Windows;
using System.Windows.Controls;
using OGT.WatchTower.Core.Config;
using OGT.WatchTower.Core.Intelligence;

namespace OGT.WatchTower.App.Views
{
    public partial class IntelligenceView : UserControl
    {
        public IntelligenceView()
        {
            InitializeComponent();
        }

        private async void CheckFile_Click(object sender, RoutedEventArgs e)
        {
            string path = TxtFilePath.Text;
            if (string.IsNullOrWhiteSpace(path)) return;

            LblFileResult.Text = "Checking...";
            
            var sentry = new CloudSentry(SettingsManager.Instance.Settings.Integrations.VirusTotalKey, "");
            int score = await sentry.CheckFileHash(path);
            
            if (score == -1)
                LblFileResult.Text = "Result: Failed (Invalid Key or File)";
            else if (score > 0)
                LblFileResult.Text = $"Result: MALICIOUS ({score} engines detected)";
            else
                LblFileResult.Text = "Result: Clean";
        }

        private void CheckIp_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder as we didn't implement IP check fully in Core yet
            LblIpResult.Text = "Result: Feature coming soon (AbuseIPDB)";
        }
    }
}
