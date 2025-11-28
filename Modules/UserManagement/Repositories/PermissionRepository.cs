using FootballField.API.Database;
using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.UserManagement.Repositories;

public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> UserHasPermissionAsync(int userId, string permissionKey)
    {
        var hasPermission = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions,
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp)
            .Join(_context.Permissions,
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p)
            .AnyAsync(p => p.PermissionKey == permissionKey);

        return hasPermission;
    }

    public async Task<IEnumerable<string>> GetUserPermissionKeysAsync(int userId)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions,
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp)
            .Join(_context.Permissions,
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p.PermissionKey)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    public async Task<IEnumerable<string>> GetUserRoleNamesAsync(int userId)
    {
        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync();

        return roles;
    }

    public async Task<Permission?> GetByKeyAsync(string permissionKey)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.PermissionKey == permissionKey);
    }

    public async Task<bool> KeyExistsAsync(string permissionKey, int? excludeId = null)
    {
        var query = _context.Permissions.Where(p => p.PermissionKey == permissionKey);
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<Permission>> GetByModuleAsync(string module)
    {
        return await _context.Permissions
            .Where(p => p.Module == module)
            .OrderBy(p => p.PermissionKey)
            .ToListAsync();
    }

    public async Task<Dictionary<string, List<Permission>>> GetAllGroupedByModuleAsync()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.PermissionKey)
            .ToListAsync();

        return permissions
            .GroupBy(p => p.Module ?? "other")
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
