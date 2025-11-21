using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Modules.FieldManagement.Dtos
{
    public class UpdateTimeSlotDto
    {
        [Required(ErrorMessage = "Giờ bắt đầu là bắt buộc")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Giờ kết thúc là bắt buộc")]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; }
    }
}
