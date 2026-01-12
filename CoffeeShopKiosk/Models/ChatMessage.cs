using System;
using System.Collections.Generic;

namespace CoffeeShopKiosk.Models
{
    public class ChatMessage
    {
        public string Role { get; set; } = "user"; // "user" or "assistant"
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        // Optional metadata for display and telemetry
        public string Source { get; set; } = "local"; // e.g., "local" or "remote"
        public double Confidence { get; set; } = 0.0;

        // Optional list of source URLs cited by the assistant
        public List<string> Sources { get; set; } = new List<string>();
    }
}