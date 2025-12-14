using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Modules.AIManagement.Dtos
{
    public class ChatRequest
    {
        [Required(ErrorMessage = "Tin nhắn không được để trống")]
        public string UserMessage { get; set; } = string.Empty;

    }
}
