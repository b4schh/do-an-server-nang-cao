using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.User;

public interface IUserRepository : IGenericRepository<UserEntity>
{
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity?> GetByPhoneAsync(string phone);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);
    Task<UserEntity?> GetUserByIdWithRoleAsync(int userId);
    Task<IEnumerable<UserEntity>> GetAllUsersWithRolesAsync();
    Task<UserEntity?> GetByIdWithRolesAsync(int userId);
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task AddUserRoleAsync(int userId, int roleId);
    Task RemoveUserRolesAsync(int userId);
    Task<(IEnumerable<UserEntity> users, int totalCount)> GetPagedUsersWithFiltersAsync(int pageIndex, int pageSize, string? keyword = null, string? role = null, byte? status = null);

    // Refresh Token methods
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task RevokeRefreshTokenAsync(string token);
    Task RevokeAllUserRefreshTokensAsync(int userId);
}
