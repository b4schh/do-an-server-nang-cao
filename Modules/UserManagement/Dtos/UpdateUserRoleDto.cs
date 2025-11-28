using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Modules.UserManagement.Dtos
{
    public class UpdateUserRoleDto
    {
        [Required(ErrorMessage = "RoleId là bắt buộc")]
        public int RoleId { get; set; }
    }
}
