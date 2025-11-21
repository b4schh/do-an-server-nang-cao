using FootballField.API.Modules.UserManagement.Dtos;

namespace FootballField.API.Modules.AuthManagement.Dtos
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
