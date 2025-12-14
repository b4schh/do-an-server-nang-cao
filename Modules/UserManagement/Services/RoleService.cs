using AutoMapper;
using FootballField.API.Modules.UserManagement.Dtos;
using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Modules.UserManagement.Repositories;

namespace FootballField.API.Modules.UserManagement.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public RoleService(IRoleRepository roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllWithCountsAsync();
        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive,
            UserCount = r.UserRoles.Count,
            PermissionCount = r.RolePermissions.Count,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });
    }

    public async Task<RoleDetailDto?> GetRoleByIdAsync(int roleId)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(roleId);
        if (role == null)
            return null;

        return new RoleDetailDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            Permissions = role.RolePermissions.Select(rp => _mapper.Map<PermissionDto>(rp.Permission)).ToList(),
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    public async Task<RoleDto?> CreateRoleAsync(CreateRoleDto dto)
    {
        // Kiểm tra tên role đã tồn tại
        if (await _roleRepository.NameExistsAsync(dto.Name))
            return null;

        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive
        };

        await _roleRepository.AddAsync(role);
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto?> UpdateRoleAsync(int roleId, UpdateRoleDto dto)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            return null;

        // Kiểm tra tên role trùng (nếu có thay đổi tên)
        if (!string.IsNullOrEmpty(dto.Name) && dto.Name != role.Name)
        {
            if (await _roleRepository.NameExistsAsync(dto.Name, roleId))
                return null;
            role.Name = dto.Name;
        }

        if (dto.Description != null)
            role.Description = dto.Description;

        if (dto.IsActive.HasValue)
            role.IsActive = dto.IsActive.Value;

        await _roleRepository.UpdateAsync(role);
        return _mapper.Map<RoleDto>(role);
    }

    public async Task<bool> DeleteRoleAsync(int roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            return false;

        // Không cho xóa role hệ thống
        if (role.Name == "Admin" || role.Name == "Owner" || role.Name == "Customer")
            return false;

        await _roleRepository.DeleteAsync(role);
        return true;
    }

    public async Task<bool> AssignPermissionsToRoleAsync(int roleId, AssignPermissionsToRoleDto dto)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            return false;

        // Xóa hết permissions cũ
        await _roleRepository.RemoveAllPermissionsAsync(roleId);

        // Thêm permissions mới
        if (dto.PermissionIds.Any())
        {
            await _roleRepository.AssignPermissionsAsync(roleId, dto.PermissionIds);
        }

        return true;
    }

    public async Task<List<PermissionDto>> GetRolePermissionsAsync(int roleId)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(roleId);
        if (role == null)
            return new List<PermissionDto>();

        return role.RolePermissions
            .Select(rp => _mapper.Map<PermissionDto>(rp.Permission))
            .ToList();
    }
}
