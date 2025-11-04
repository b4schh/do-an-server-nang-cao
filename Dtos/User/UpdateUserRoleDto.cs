using System.ComponentModel.DataAnnotations;
using FootballField.API.Entities;

namespace FootballField.API.Dtos.User
{
    public class UpdateUserRoleDto
    {
        [Required(ErrorMessage = "Role là bắt buộc")]
        public UserRole Role { get; set; }
    }
}