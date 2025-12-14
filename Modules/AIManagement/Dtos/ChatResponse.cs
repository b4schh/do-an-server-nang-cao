using System;

namespace FootballField.API.Modules.AIManagement.Dtos
{
    public class ChatResponse
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string Role { get; set; } = "assistant";
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? ImageUrl { get; set; }
    }
}
