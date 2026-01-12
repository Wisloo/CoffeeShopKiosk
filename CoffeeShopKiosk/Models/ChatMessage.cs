using System;

namespace CoffeeShopKiosk.Models
{
    public class ChatMessage
    {
        public string Role { get; set; } = "user"; // "user" or "assistant"
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}