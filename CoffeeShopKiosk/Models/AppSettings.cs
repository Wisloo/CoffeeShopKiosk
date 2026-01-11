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
        public List<PresetOrder> Presets { get; set; } = new List<PresetOrder>();
    }
}