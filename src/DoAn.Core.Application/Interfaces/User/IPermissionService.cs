namespace DoAn.Core.Application.Interfaces.User;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, string permissionKey);
    Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
    Task<IEnumerable<string>> GetUserRolesAsync(int userId);
}
