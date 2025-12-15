using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Modules.FieldManagement.Dtos
{
    /// <summary>
    /// DTO for cloning an existing field with all its timeslots
    /// </summary>
    public class CloneFieldDto
    {
        [Required(ErrorMessage = "Tên sân mới là bắt buộc")]
        [MaxLength(100)]
        public string NewFieldName { get; set; } = null!;

        public bool IncludeTimeSlots { get; set; } = true;
    }

    /// <summary>
    /// DTO for batch adding timeslots to multiple fields
    /// </summary>
    public class BatchAddTimeSlotsDto
    {
        [Required(ErrorMessage = "Danh sách Field IDs là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 sân")]
        public List<int> FieldIds { get; set; } = new();

        [Required(ErrorMessage = "Danh sách khung giờ là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 khung giờ")]
        public List<TimeSlotTemplateItem> TimeSlots { get; set; } = new();
    }

    public class TimeSlotTemplateItem
    {
        [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Giờ kết thúc là bắt buộc")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
        public decimal Price { get; set; }
    }
}
