using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Modules.UserManagement.Dtos
{
    public class UpdateUserStatusDto
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Range(0, 2, ErrorMessage = "Trạng thái không hợp lệ (0: Inactive, 1: Active, 2: Banned)")]
        public byte Status { get; set; }
    }
}
