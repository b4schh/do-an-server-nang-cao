using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Dtos.Booking
{
    public class RejectBookingDto
    {
        [MaxLength(255, ErrorMessage = "Lý do không được vượt quá 255 ký tự")]
        public string? Reason { get; set; }
    }
}
