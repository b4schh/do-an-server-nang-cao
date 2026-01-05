using AutoMapper;
using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.Interfaces.User;
using DoAn.Core.Application.DTOs.User;

namespace DoAn.Core.Application.Services.User;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionKey)
    {
        return await _permissionRepository.UserHasPermissionAsync(userId, permissionKey);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
    {
        return await _permissionRepository.GetUserPermissionKeysAsync(userId);
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
    {
        return await _permissionRepository.GetUserRoleNamesAsync(userId);
    }
}
