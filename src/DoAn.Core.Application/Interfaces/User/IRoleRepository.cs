using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.User;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetByIdWithPermissionsAsync(int roleId);
    Task<IEnumerable<Role>> GetAllWithCountsAsync();
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task AssignPermissionsAsync(int roleId, List<int> permissionIds);
    Task RemoveAllPermissionsAsync(int roleId);
}
