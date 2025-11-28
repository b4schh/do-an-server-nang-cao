using AutoMapper;
using FootballField.API.Modules.UserManagement.Dtos;
using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Modules.UserManagement.Repositories;

namespace FootballField.API.Modules.UserManagement.Services;

public class PermissionManagementService : IPermissionManagementService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IMapper _mapper;

    public PermissionManagementService(IPermissionRepository permissionRepository, IMapper mapper)
    {
        _permissionRepository = permissionRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _permissionRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(int permissionId)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        return permission == null ? null : _mapper.Map<PermissionDto>(permission);
    }

    public async Task<IEnumerable<PermissionsByModuleDto>> GetPermissionsGroupedByModuleAsync()
    {
        var grouped = await _permissionRepository.GetAllGroupedByModuleAsync();
        return grouped.Select(g => new PermissionsByModuleDto
        {
            Module = g.Key,
            Permissions = _mapper.Map<List<PermissionDto>>(g.Value)
        }).OrderBy(x => x.Module);
    }

    public async Task<IEnumerable<PermissionDto>> GetPermissionsByModuleAsync(string module)
    {
        var permissions = await _permissionRepository.GetByModuleAsync(module);
        return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
    }

    public async Task<PermissionDto?> CreatePermissionAsync(CreatePermissionDto dto)
    {
        // Kiểm tra permission key đã tồn tại
        if (await _permissionRepository.KeyExistsAsync(dto.PermissionKey))
            return null;

        var permission = new Permission
        {
            PermissionKey = dto.PermissionKey,
            Description = dto.Description,
            Module = dto.Module
        };

        await _permissionRepository.AddAsync(permission);
        return _mapper.Map<PermissionDto>(permission);
    }

    public async Task<PermissionDto?> UpdatePermissionAsync(int permissionId, UpdatePermissionDto dto)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission == null)
            return null;

        // Kiểm tra permission key trùng (nếu có thay đổi)
        if (!string.IsNullOrEmpty(dto.PermissionKey) && dto.PermissionKey != permission.PermissionKey)
        {
            if (await _permissionRepository.KeyExistsAsync(dto.PermissionKey, permissionId))
                return null;
            permission.PermissionKey = dto.PermissionKey;
        }

        if (dto.Description != null)
            permission.Description = dto.Description;

        if (dto.Module != null)
            permission.Module = dto.Module;

        await _permissionRepository.UpdateAsync(permission);
        return _mapper.Map<PermissionDto>(permission);
    }

    public async Task<bool> DeletePermissionAsync(int permissionId)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission == null)
            return false;

        await _permissionRepository.DeleteAsync(permission);
        return true;
    }
}
