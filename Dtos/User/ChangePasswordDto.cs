

namespace FootballField.API.Dtos.User  {


public class ChangePasswordDto
{
    // Thuộc tính để lưu trữ mật khẩu hiện tại của người dùng
    public string CurrentPassword { get; set; }

    // Thuộc tính để lưu trữ mật khẩu mới mà người dùng muốn đặt
    public string NewPassword { get; set; }

    // Constructor (hàm tạo) để khởi tạo đối tượng
    public ChangePasswordDto(string currentPassword, string newPassword)
    {
        CurrentPassword = currentPassword;
        NewPassword = newPassword;
    }

    // Constructor mặc định (không đối số)
    public ChangePasswordDto() { }
}

}