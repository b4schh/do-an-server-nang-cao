using DoAn.Core.Application.DTOs.User;

namespace DoAn.Core.Application.Interfaces.User;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<(IEnumerable<UserDto> users, int totalCount)> GetPagedUsersAsync(int pageIndex, int pageSize, string? keyword = null, string? role = null, byte? status = null);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task UpdateUserRoleAsync(int id, UpdateUserRoleDto updateUserRoleDto);
    Task SoftDeleteUserAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<UserResponseDto> UpdateAvatarAsync(int userId, string? avatarUrl);
    Task<UserResponseDto> UpdateUserProfileAsync(int id, UpdateUserProfileDto dto);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task UpdateUserStatusAsync(int userId, byte status);
    Task<Dictionary<string, int>> GetUserStatisticsByRoleAsync();
}