using System;
using System.IO;

namespace CoffeeShopKiosk.Services
{
    public class TelemetryService
    {
        private readonly string _path;
        public TelemetryService()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            _path = Path.Combine(dir, "telemetry.log");
        }

        public void RecordFeedback(string message, string source, bool helpful)
        {
            try
            {
                File.AppendAllText(_path, $"{DateTimeOffset.Now:u} | Feedback | Source:{source} | Helpful:{helpful} | Message:{message.Replace('\n',' ')}\n");
            }
            catch { }
        }

        public void RecordRawResponse(string raw)
        {
            try
            {
                File.AppendAllText(_path, $"{DateTimeOffset.Now:u} | RawResponse | {raw.Replace('\n',' ')}\n");
            }
            catch { }
        }

        public string? GetLastRawResponse()
        {
            try
            {
                if (!File.Exists(_path)) return null;
                var lines = File.ReadAllLines(_path);
                for (int i = lines.Length - 1; i >= 0; i--)
                {
                    var l = lines[i];
                    if (l.Contains("| RawResponse |")) return l;
                }
                return null;
            }
            catch { return null; }
        }
    }
}