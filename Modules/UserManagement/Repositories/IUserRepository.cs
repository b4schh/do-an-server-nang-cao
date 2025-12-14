using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Shared.Base;

namespace FootballField.API.Modules.UserManagement.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phone);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetUserByIdWithRoleAsync(int userId);
        Task<IEnumerable<User>> GetAllUsersWithRolesAsync();
        Task<User?> GetByIdWithRolesAsync(int userId);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task AddUserRoleAsync(int userId, int roleId);
        Task RemoveUserRolesAsync(int userId);

        // Refresh Token methods
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserRefreshTokensAsync(int userId);
    }
}
