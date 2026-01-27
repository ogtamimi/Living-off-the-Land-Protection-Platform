using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OGT.WatchTower.Core.Config
{
    public class SettingsManager
    {
        private static SettingsManager _instance;
        public static SettingsManager Instance => _instance ??= new SettingsManager();

        public AppSettings Settings { get; private set; }
        private string _configPath;

        private SettingsManager()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "settings.json");
            Load();
        }

        public void Load()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    string json = File.ReadAllText(_configPath);
                    Settings = JsonSerializer.Deserialize<AppSettings>(json);
                }
                catch (Exception)
                {
                    // Fallback to default
                    Settings = new AppSettings();
                }
            }
            else
            {
                Settings = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }

    public class AppSettings
    {
        [JsonPropertyName("general")]
        public GeneralSettings General { get; set; } = new GeneralSettings();

        [JsonPropertyName("telemetry")]
        public TelemetrySettings Telemetry { get; set; } = new TelemetrySettings();

        [JsonPropertyName("detection")]
        public DetectionSettings Detection { get; set; } = new DetectionSettings();

        [JsonPropertyName("integrations")]
        public IntegrationSettings Integrations { get; set; } = new IntegrationSettings();

        [JsonPropertyName("response")]
        public ResponseSettings Response { get; set; } = new ResponseSettings();

        [JsonPropertyName("whitelist")]
        public List<string> Whitelist { get; set; } = new List<string>();
    }

    public class GeneralSettings
    {
        [JsonPropertyName("dark_mode")]
        public bool DarkMode { get; set; } = true;
    }

    public class TelemetrySettings
    {
        [JsonPropertyName("sysmon_enabled")]
        public bool SysmonEnabled { get; set; } = true;
    }

    public class DetectionSettings
    {
        [JsonPropertyName("sigma_enabled")]
        public bool SigmaEnabled { get; set; } = true;
    }

    public class IntegrationSettings
    {
        [JsonPropertyName("virustotal_key")]
        public string VirusTotalKey { get; set; } = "";

        [JsonPropertyName("abuseipdb_key")]
        public string AbuseIpDbKey { get; set; } = "";
    }

    public class ResponseSettings
    {
        [JsonPropertyName("auto_kill")]
        public bool AutoKill { get; set; } = false;

        [JsonPropertyName("auto_suspend")]
        public bool AutoSuspend { get; set; } = false;
    }
}
