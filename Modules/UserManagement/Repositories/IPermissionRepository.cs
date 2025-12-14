using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Shared.Base;

namespace FootballField.API.Modules.UserManagement.Repositories;

public interface IPermissionRepository : IGenericRepository<Permission>
{
    Task<bool> UserHasPermissionAsync(int userId, string permissionKey);
    Task<IEnumerable<string>> GetUserPermissionKeysAsync(int userId);
    Task<IEnumerable<string>> GetUserRoleNamesAsync(int userId);
    Task<Permission?> GetByKeyAsync(string permissionKey);
    Task<bool> KeyExistsAsync(string permissionKey, int? excludeId = null);
    Task<IEnumerable<Permission>> GetByModuleAsync(string module);
    Task<Dictionary<string, List<Permission>>> GetAllGroupedByModuleAsync();
}
