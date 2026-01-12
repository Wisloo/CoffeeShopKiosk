using System.Collections.Generic;

namespace CoffeeShopKiosk.Models
{
    public class StudyResponse
    {
        public string Summary { get; set; } = string.Empty;
        public List<string> Tips { get; set; } = new List<string>();
        public string Action { get; set; } = string.Empty;
        public double Confidence { get; set; } = 0.0;
        public string Source { get; set; } = "remote";
        public List<string> Sources { get; set; } = new List<string>();
    }
}