using System.ComponentModel.DataAnnotations;
using FootballField.API.Entities;

namespace FootballField.API.Dtos.Booking
{
    public class UpdateBookingDto
    {
        [Required(ErrorMessage = "Ngày đặt sân là bắt buộc")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "TimeSlotId là bắt buộc")]
        public int TimeSlotId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tiền cọc phải lớn hơn hoặc bằng 0")]
        public int DepositAmount { get; set; }

        [Required(ErrorMessage = "Tổng tiền là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn 0")]
        public int TotalAmount { get; set; }

        public BookingStatus BookingStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Note { get; set; }
    }
}
