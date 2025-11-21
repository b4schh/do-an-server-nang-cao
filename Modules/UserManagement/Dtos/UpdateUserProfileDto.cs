using System.ComponentModel.DataAnnotations;

namespace FootballField.API.Modules.UserManagement.Dtos
{
    public class UpdateUserProfileDto
    {
        [StringLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
        public string? FirstName { get; set; }

        [StringLength(100, ErrorMessage = "Họ không được quá 100 ký tự")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        public string? Phone { get; set; }
    }
}
