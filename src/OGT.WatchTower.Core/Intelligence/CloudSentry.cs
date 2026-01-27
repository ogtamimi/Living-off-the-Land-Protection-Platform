using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OGT.WatchTower.Core.Intelligence
{
    public class CloudSentry
    {
        private string _vtKey;
        private string _abuseKey;
        private HttpClient _client;

        public CloudSentry(string vtKey, string abuseKey)
        {
            _vtKey = vtKey;
            _abuseKey = abuseKey;
            _client = new HttpClient();
        }

        public async Task<int> CheckFileHash(string filePath)
        {
            if (string.IsNullOrEmpty(_vtKey) || !File.Exists(filePath)) return -1;

            try
            {
                string hash = GetFileHash(filePath);
                string url = $"https://www.virustotal.com/api/v3/files/{hash}";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("x-apikey", _vtKey);

                var response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var malicious = doc.RootElement
                        .GetProperty("data")
                        .GetProperty("attributes")
                        .GetProperty("last_analysis_stats")
                        .GetProperty("malicious")
                        .GetInt32();
                    
                    return malicious;
                }
            }
            catch { }
            return -1;
        }

        private string GetFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
