using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Shared.Dtos.BookingManagement
{
    public class CreateBookingDto
    {
        [Required(ErrorMessage = "FieldId là bắt buộc")]
        public int FieldId { get; set; }

        [Required(ErrorMessage = "TimeSlotId là bắt buộc")]
        public int TimeSlotId { get; set; }

        [Required(ErrorMessage = "BookingDate là bắt buộc")]
        public DateTime BookingDate { get; set; }

        [MaxLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
        public string? Note { get; set; }
    }
}
