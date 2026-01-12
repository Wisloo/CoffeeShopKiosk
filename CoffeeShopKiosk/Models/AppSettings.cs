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
        // Visual theme and accessibility
        public string VisualTheme { get; set; } = "Cafe"; // Main theme is Cafe
        public bool DoNotDisturb { get; set; } = false;
        public bool ReduceMotion { get; set; } = true;
        public int PomodoroWorkMinutes { get; set; } = 25;
        public int PomodoroBreakMinutes { get; set; } = 5;

        // Focus Spotlight & UI toggles
        public bool SpotlightEnabled { get; set; } = false;
        public bool HideProductImages { get; set; } = false;

        // Visual effect toggle
        public bool VisualEffectsEnabled { get; set; } = true;

        public List<PresetOrder> Presets { get; set; } = new List<PresetOrder>();

        // AI feature toggles
        public bool EnableAIStudyAssistant { get; set; } = false;
    }
}