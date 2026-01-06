using DoAn.Core.Application.Interfaces.User;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.User;

public class RoleRepository : GenericRepository<RoleEntity>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RoleEntity?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<RoleEntity?> GetByIdWithPermissionsAsync(int roleId)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<IEnumerable<RoleEntity>> GetAllWithCountsAsync()
    {
        return await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Roles.Where(r => r.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task AssignPermissionsAsync(int roleId, List<int> permissionIds)
    {
        var rolePermissions = permissionIds.Select(permissionId => new RolePermissionEntity
        {
            RoleId = roleId,
            PermissionId = permissionId
        }).ToList();

        await _context.RolePermissions.AddRangeAsync(rolePermissions);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAllPermissionsAsync(int roleId)
    {
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(rolePermissions);
        await _context.SaveChangesAsync();
    }
}
