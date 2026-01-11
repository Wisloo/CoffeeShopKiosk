using System;
using System.IO;
using System.Windows.Media;

namespace CoffeeShopKiosk.Services
{
    public static class AmbientSoundService
    {
        private static MediaPlayer _player = new MediaPlayer();

        public static void Play(string soundName, double volume = 0.5)
        {
            try
            {
                _player.Volume = Math.Max(0, Math.Min(1, volume));
                var path = GetSoundPath(soundName);
                if (File.Exists(path))
                {
                    _player.Open(new Uri(path, UriKind.Absolute));
                    _player.MediaEnded += (s, e) => _player.Position = TimeSpan.Zero;
                    _player.Play();
                }
            }
            catch { /* swallow for now */ }
        }

        public static void Stop()
        {
            try
            {
                _player.Stop();
            }
            catch { }
        }

        private static string GetSoundPath(string name)
        {
            // For now support 'Rain' only and expect a file in /Resources/sounds
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "Resources", "sounds", name + ".mp3");
        }
    }
}