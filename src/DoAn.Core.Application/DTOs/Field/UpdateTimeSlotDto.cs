using System.ComponentModel.DataAnnotations;

namespace DoAn.Core.Application.DTOs.Field;

public class UpdateTimeSlotDto
{
    [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Giờ kết thúc là bắt buộc")]
    public TimeSpan EndTime { get; set; }

    public bool? IsActive { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá tiền không hợp lệ")]
    public decimal? Price { get; set; }
}
