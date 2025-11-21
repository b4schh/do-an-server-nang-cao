using FootballField.API.Database;
using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.UserManagement.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }


        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Phone == phone && !u.IsDeleted);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _dbSet.Where(u => u.Role == role && !u.IsDeleted).ToListAsync();
        }

        public async Task<User?> GetUserByIdWithRoleAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }
    }
}
