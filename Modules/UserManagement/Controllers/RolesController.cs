using FootballField.API.Modules.UserManagement.Dtos;
using FootballField.API.Modules.UserManagement.Services;
using FootballField.API.Shared.Dtos;
using FootballField.API.Shared.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Modules.UserManagement.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Lấy danh sách tất cả roles (Admin only)
    /// </summary>
    [HttpGet]
    [HasPermission("role.view_all")]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(ApiResponse<IEnumerable<RoleDto>>.Ok(roles, "Lấy danh sách roles thành công"));
    }

    /// <summary>
    /// Lấy chi tiết role theo ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [HasPermission("role.view_all")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
            return NotFound(ApiResponse<string>.Fail("Không tìm thấy role", 404));

        return Ok(ApiResponse<RoleDetailDto>.Ok(role, "Lấy thông tin role thành công"));
    }

    /// <summary>
    /// Tạo role mới (Admin only)
    /// </summary>
    [HttpPost]
    [HasPermission("role.create")]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        var role = await _roleService.CreateRoleAsync(dto);
        if (role == null)
            return BadRequest(ApiResponse<string>.Fail("Tên role đã tồn tại", 400));

        return CreatedAtAction(nameof(GetById), new { id = role.Id }, 
            ApiResponse<RoleDto>.Ok(role, "Tạo role thành công"));
    }

    /// <summary>
    /// Cập nhật role (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("role.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _roleService.UpdateRoleAsync(id, dto);
        if (role == null)
            return BadRequest(ApiResponse<string>.Fail("Không tìm thấy role hoặc tên role đã tồn tại", 400));

        return Ok(ApiResponse<RoleDto>.Ok(role, "Cập nhật role thành công"));
    }

    /// <summary>
    /// Xóa role (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("role.delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        if (!result)
            return BadRequest(ApiResponse<string>.Fail("Không thể xóa role hệ thống hoặc role không tồn tại", 400));

        return Ok(ApiResponse<string>.Ok(null, "Xóa role thành công"));
    }

    /// <summary>
    /// Gán permissions cho role (Admin only)
    /// </summary>
    [HttpPost("{id}/permissions")]
    [HasPermission("role.edit")]
    public async Task<IActionResult> AssignPermissions(int id, [FromBody] AssignPermissionsToRoleDto dto)
    {
        var result = await _roleService.AssignPermissionsToRoleAsync(id, dto);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("Không tìm thấy role", 404));

        return Ok(ApiResponse<string>.Ok(null, "Gán permissions thành công"));
    }

    /// <summary>
    /// Lấy danh sách permissions của role (Admin only)
    /// </summary>
    [HttpGet("{id}/permissions")]
    [HasPermission("role.view_all")]
    public async Task<IActionResult> GetRolePermissions(int id)
    {
        var permissions = await _roleService.GetRolePermissionsAsync(id);
        return Ok(ApiResponse<List<PermissionDto>>.Ok(permissions, "Lấy danh sách permissions thành công"));
    }
}
