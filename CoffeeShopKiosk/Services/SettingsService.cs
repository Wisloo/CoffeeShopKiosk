using System;
using System.IO;
using System.Text.Json;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Services
{
    public class SettingsService
    {
        private readonly string _path;
        private AppSettings _settings;

        public SettingsService()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            _path = Path.Combine(dir, "appsettings.json");
            _settings = Load();
        }

        public AppSettings Settings => _settings;

        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    var s = new AppSettings();
                    Save(s);
                    return s;
                }
                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            _settings = settings ?? new AppSettings();
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }

        public void Update(Action<AppSettings> update)
        {
            update?.Invoke(_settings);
            Save(_settings);
        }
    }
}