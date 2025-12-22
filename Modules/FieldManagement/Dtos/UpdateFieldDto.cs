using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Modules.FieldManagement.Dtos

{
    public class UpdateFieldDto
    {
        [Required(ErrorMessage = "Tên sân là bắt buộc")]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Loại kích thước sân là bắt buộc")]
        public string FieldSize { get; set; } = null!;

        [Required(ErrorMessage = "Loại mặt sân là bắt buộc")]

        public string? SurfaceType { get; set; }
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}
