using System.ComponentModel.DataAnnotations;

namespace DoAn.Core.Application.DTOs.User;

public class UpdateUserRoleDto
{
    [Required(ErrorMessage = "RoleId là bắt buộc")]
    public int RoleId { get; set; }
}
