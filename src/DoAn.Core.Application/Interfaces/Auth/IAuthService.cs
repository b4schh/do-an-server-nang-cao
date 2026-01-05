using DoAn.Core.Application.DTOs.Auth;
using DoAn.Core.Application.DTOs.User;

namespace DoAn.Core.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> RegisterAsync(RegisterRequest request);
    Task<RefreshTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserDto?> GetCurrentUserAsync(int userId);
    string HashPassword(string password);
    Task<bool> ValidatePasswordAsync(string password, string hashedPassword);
    bool VerifyPassword(string password, string hashedPassword);
}
