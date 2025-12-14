using System.ComponentModel.DataAnnotations;
using FootballField.API.Modules.UserManagement.Entities;

namespace FootballField.API.Modules.UserManagement.Dtos
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Họ là bắt buộc")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = null!;
        
        public string? AvatarUrl { get; set; }
    }
}
