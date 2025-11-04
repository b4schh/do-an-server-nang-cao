using System.ComponentModel.DataAnnotations;
using FootballField.API.Entities;

namespace FootballField.API.Dtos.Complex
{
    public class UpdateComplexDto
    {
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

        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        public string? Description { get; set; }

        public ComplexStatus Status { get; set; }
        
        public bool IsActive { get; set; }
    }
}
