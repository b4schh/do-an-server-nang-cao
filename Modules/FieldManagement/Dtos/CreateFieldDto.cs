using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Modules.FieldManagement.Dtos
{
    public class CreateFieldDto
    {
        [Required(ErrorMessage = "ComplexId là bắt buộc")]
        public int ComplexId { get; set; }

        [Required(ErrorMessage = "Tên sân là bắt buộc")]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Loại sân là bắt buộc")]
        public string FieldType { get; set; } = null!;

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public int PricePerHour { get; set; }

        public string? Description { get; set; }
    }
}
