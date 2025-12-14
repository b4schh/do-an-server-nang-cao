using FootballField.API.Modules.UserManagement.Dtos;

namespace FootballField.API.Modules.UserManagement.Services;

public interface IPermissionManagementService
{
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<PermissionDto?> GetPermissionByIdAsync(int permissionId);
    Task<IEnumerable<PermissionsByModuleDto>> GetPermissionsGroupedByModuleAsync();
    Task<IEnumerable<PermissionDto>> GetPermissionsByModuleAsync(string module);
    Task<PermissionDto?> CreatePermissionAsync(CreatePermissionDto dto);
    Task<PermissionDto?> UpdatePermissionAsync(int permissionId, UpdatePermissionDto dto);
    Task<bool> DeletePermissionAsync(int permissionId);
}
