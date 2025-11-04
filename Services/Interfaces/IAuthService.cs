using FootballField.API.Dtos.Auth;
using FootballField.API.Dtos.User;

namespace FootballField.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<LoginResponse?> RegisterAsync(RegisterRequest request);
        Task<UserDto?> GetCurrentUserAsync(int userId);
        Task<bool> ValidatePasswordAsync(string password, string hashedPassword);
        string HashPassword(string password);
    }
}
