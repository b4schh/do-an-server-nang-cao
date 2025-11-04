using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Dtos.Field
{
    public class UpdateFieldDto
    {
        [Required(ErrorMessage = "Tên sân là bắt buộc")]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Loại sân là bắt buộc")]
        public string FieldType { get; set; } = null!;

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public int PricePerHour { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}