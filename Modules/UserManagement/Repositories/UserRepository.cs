using FootballField.API.Database;
using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Shared.Base;
using FootballField.API.Shared.Utils;
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

        public async Task<User?> GetUserByIdWithRoleAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }

        public async Task<IEnumerable<User>> GetAllUsersWithRolesAsync()
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<User?> GetByIdWithRolesAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task AddUserRoleAsync(int userId, int roleId)
        {
            var userRole = new UserRole { UserId = userId, RoleId = roleId };
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserRolesAsync(int userId)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
            _context.UserRoles.RemoveRange(userRoles);
            await _context.SaveChangesAsync();
        }

        // ======================= REFRESH TOKEN METHODS =======================

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            Console.WriteLine($"[AddRefreshToken] Adding new token for user {refreshToken.UserId}: {refreshToken.Token.Substring(0, 10)}...");
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[AddRefreshToken] SaveChanges completed, Token ID: {refreshToken.Id}");
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = TimeZoneHelper.VietnamNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(int userId)
        {
            var refreshTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            if (refreshTokens.Any())
            {
                Console.WriteLine($"[RevokeAllUserRefreshTokens] Revoking {refreshTokens.Count} token(s) for user {userId}");
                
                foreach (var token in refreshTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = TimeZoneHelper.VietnamNow;
                    Console.WriteLine($"[RevokeAllUserRefreshTokens] Revoked token: {token.Token.Substring(0, 10)}...");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[RevokeAllUserRefreshTokens] SaveChanges completed");
            }
            else
            {
                Console.WriteLine($"[RevokeAllUserRefreshTokens] No active tokens found for user {userId}");
            }
        }
    }
}
