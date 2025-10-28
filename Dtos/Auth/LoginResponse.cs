using FootballField.API.Dtos.User;

namespace FootballField.API.Dtos.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
