using FootballField.API.Modules.AuthManagement.Dtos;
using FootballField.API.Modules.UserManagement.Dtos;

namespace FootballField.API.Modules.AuthManagement.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<LoginResponse?> RegisterAsync(RegisterRequest request);
        Task<UserDto?> GetCurrentUserAsync(int userId);
        string HashPassword(string password);
        Task<bool> ValidatePasswordAsync(string password, string hashedPassword);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
