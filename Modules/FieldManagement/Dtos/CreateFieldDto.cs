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
        public string FieldSize { get; set; } = null!;

        public string? SurfaceType { get; set; }

        public string? Description { get; set; }
    }
}
