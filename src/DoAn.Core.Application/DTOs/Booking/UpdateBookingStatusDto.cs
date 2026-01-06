using System.ComponentModel.DataAnnotations;

namespace DoAn.Core.Application.DTOs.Booking;

public class UpdateBookingStatusDto
{
    [Required(ErrorMessage = "BookingStatus là bắt buộc")]
    public BookingStatus Status { get; set; }

    [MaxLength(255, ErrorMessage = "Lý do không được vượt quá 255 ký tự")]
    public string? Reason { get; set; }
}
