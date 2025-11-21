

using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Shared.Base;

namespace FootballField.API.Modules.UserManagement.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phone);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
        Task<User?> GetUserByIdWithRoleAsync(int userId);
    }
}
