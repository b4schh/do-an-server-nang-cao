using DoAn.Core.Application.DTOs.User;

namespace DoAn.Core.Application.Interfaces.User;

public interface IPermissionManagementService
{
    Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
    Task<(IEnumerable<PermissionDto> items, int totalCount)> GetPagedPermissionsAsync(int pageIndex, int pageSize, string? keyword = null, string? module = null);
    Task<PermissionDto?> GetPermissionByIdAsync(int permissionId);
    Task<IEnumerable<PermissionsByModuleDto>> GetPermissionsGroupedByModuleAsync();
    Task<IEnumerable<PermissionDto>> GetPermissionsByModuleAsync(string module);
    Task<PermissionDto?> CreatePermissionAsync(CreatePermissionDto dto);
    Task<PermissionDto?> UpdatePermissionAsync(int permissionId, UpdatePermissionDto dto);
    Task<bool> DeletePermissionAsync(int permissionId);
}
