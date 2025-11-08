using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Dtos.Booking
{
    public class CreateBookingDto
    {
        [Required(ErrorMessage = "FieldId là bắt buộc")]
        public int FieldId { get; set; }

        [Required(ErrorMessage = "CustomerId là bắt buộc")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "OwnerId là bắt buộc")]
        public int OwnerId { get; set; }

        [Required(ErrorMessage = "Ngày đặt sân là bắt buộc")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "TimeSlotId là bắt buộc")]
        public int TimeSlotId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tiền cọc phải lớn hơn hoặc bằng 0")]
        public int DepositAmount { get; set; }

        [Required(ErrorMessage = "Tổng tiền là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0")]
        public int TotalAmount { get; set; }

        public string? PaymentMethod { get; set; }
        public string? Note { get; set; }
    }
}
