using System.Windows;
using System.Windows.Controls;
using OGT.WatchTower.Core.Config;

namespace OGT.WatchTower.App.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var s = SettingsManager.Instance.Settings;
            ChkAutoSuspend.IsChecked = s.Response.AutoSuspend;
            ChkAutoKill.IsChecked = s.Response.AutoKill;
            ChkSysmon.IsChecked = s.Telemetry.SysmonEnabled;
            
            TxtVirusTotal.Text = s.Integrations.VirusTotalKey;
            TxtAbuseIpDb.Text = s.Integrations.AbuseIpDbKey;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var s = SettingsManager.Instance.Settings;
            s.Response.AutoSuspend = ChkAutoSuspend.IsChecked ?? false;
            s.Response.AutoKill = ChkAutoKill.IsChecked ?? false;
            s.Telemetry.SysmonEnabled = ChkSysmon.IsChecked ?? true;
            
            s.Integrations.VirusTotalKey = TxtVirusTotal.Text;
            s.Integrations.AbuseIpDbKey = TxtAbuseIpDb.Text;
            
            SettingsManager.Instance.Save();
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
