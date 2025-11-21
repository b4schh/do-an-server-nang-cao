using System.ComponentModel.DataAnnotations;
using FootballField.API.Modules.UserManagement.Entities;

namespace FootballField.API.Modules.UserManagement.Dtos
{
    public class UpdateUserRoleDto
    {
        [Required(ErrorMessage = "Role là bắt buộc")]
        public UserRole Role { get; set; }
    }
}
