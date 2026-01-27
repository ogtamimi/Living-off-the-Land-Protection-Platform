using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OGT.WatchTower.Core.Logging
{
    public class Logger
    {
        private static Logger _instance;
        public static Logger Instance => _instance ??= new Logger();

        private string _logPath;
        private object _lock = new object();

        private Logger()
        {
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            _logPath = Path.Combine(logDir, "watchtower.log");
        }

        public void Log(string type, string message, string details = "")
        {
            try
            {
                var entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{type}|{message}|{details}{Environment.NewLine}";
                lock (_lock)
                {
                    File.AppendAllText(_logPath, entry);
                }
            }
            catch { }
        }

        public List<LogEntry> GetLogs()
        {
            var logs = new List<LogEntry>();
            if (!File.Exists(_logPath)) return logs;

            try
            {
                var lines = File.ReadAllLines(_logPath);
                foreach (var line in lines.Reverse()) // Newest first
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4)
                    {
                        logs.Add(new LogEntry
                        {
                            Timestamp = parts[0],
                            Type = parts[1],
                            Message = parts[2],
                            Details = parts[3]
                        });
                    }
                }
            }
            catch { }

            return logs;
        }
    }

    public class LogEntry
    {
        public string Timestamp { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
