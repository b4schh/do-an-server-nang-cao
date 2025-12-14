using FootballField.API.Modules.UserManagement.Dtos;

namespace FootballField.API.Modules.UserManagement.Services;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<RoleDetailDto?> GetRoleByIdAsync(int roleId);
    Task<RoleDto?> CreateRoleAsync(CreateRoleDto dto);
    Task<RoleDto?> UpdateRoleAsync(int roleId, UpdateRoleDto dto);
    Task<bool> DeleteRoleAsync(int roleId);
    Task<bool> AssignPermissionsToRoleAsync(int roleId, AssignPermissionsToRoleDto dto);
    Task<List<PermissionDto>> GetRolePermissionsAsync(int roleId);
}
