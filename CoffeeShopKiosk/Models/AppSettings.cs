using System.Collections.Generic;

namespace CoffeeShopKiosk.Models
{
    public class PresetOrder
    {
        public string Name { get; set; }
        public List<int> ProductIds { get; set; } = new List<int>();
    }

    public class AppSettings
    {
        public bool StudyMode { get; set; } = false;
        public bool DoNotDisturb { get; set; } = false;
        public bool ReduceMotion { get; set; } = true; // default to reduced motion when StudyMode enabled
        public int PomodoroWorkMinutes { get; set; } = 25;
        public int PomodoroBreakMinutes { get; set; } = 5;
        public bool AutoStartPomodoroOnStudyMode { get; set; } = true;
        // Focus Spotlight & UI toggles
        public bool SpotlightEnabled { get; set; } = true;
        public bool HideProductImages { get; set; } = false;

        // Ambient focus sound settings
        public bool AmbientSoundEnabled { get; set; } = false;
        public string AmbientSoundChoice { get; set; } = "Rain"; // simple default
        public double AmbientSoundVolume { get; set; } = 0.5;
        public List<PresetOrder> Presets { get; set; } = new List<PresetOrder>();
    }
}