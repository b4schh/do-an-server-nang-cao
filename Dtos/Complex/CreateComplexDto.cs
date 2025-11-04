using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Dtos.Complex
{
    public class CreateComplexDto
    {
        [Required(ErrorMessage = "OwnerId là bắt buộc")]
        public int OwnerId { get; set; }

        [Required(ErrorMessage = "Tên sân là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Street { get; set; }
        public string? Ward { get; set; }
        public string? Province { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        public TimeSpan? OpeningTime { get; set; }
        public TimeSpan? ClosingTime { get; set; }
        public string? Description { get; set; }
    }
}
