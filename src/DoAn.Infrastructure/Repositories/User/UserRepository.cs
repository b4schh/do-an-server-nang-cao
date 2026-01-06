using DoAn.Core.Application.Interfaces.User;
using DoAn.Core.Application.Common.Utils;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.User;

public class UserRepository : GenericRepository<UserEntity>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }


    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<UserEntity?> GetByPhoneAsync(string phone)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Phone == phone && !u.IsDeleted);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<UserEntity?> GetUserByIdWithRoleAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<IEnumerable<UserEntity>> GetAllUsersWithRolesAsync()
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<UserEntity?> GetByIdWithRolesAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<RoleEntity?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task AddUserRoleAsync(int userId, int roleId)
    {
        var userRole = new UserRoleEntity { UserId = userId, RoleId = roleId };
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

    public async Task<(IEnumerable<UserEntity> users, int totalCount)> GetPagedUsersWithFiltersAsync(
        int pageIndex,
        int pageSize,
        string? keyword = null,
        string? role = null,
        byte? status = null)
    {
        var query = _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .AsQueryable();

        // Filter by keyword (search in name, email, phone)
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(u =>
                (u.FirstName + " " + u.LastName).Contains(keyword) ||
                (u.Email != null && u.Email.Contains(keyword)) ||
                (u.Phone != null && u.Phone.Contains(keyword))
            );
        }

        // Filter by role
        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));
        }

        // Filter by status
        if (status.HasValue)
        {
            query = query.Where(u => (byte)u.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    // ======================= REFRESH TOKEN METHODS =======================

    public async Task<RefreshTokenEntity?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task AddRefreshTokenAsync(RefreshTokenEntity refreshToken)
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
