using FootballField.API.Modules.UserManagement.Dtos;
using FootballField.API.Modules.UserManagement.Services;
using FootballField.API.Shared.Dtos;
using FootballField.API.Shared.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Modules.UserManagement.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionManagementService _permissionService;

    public PermissionsController(IPermissionManagementService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Lấy danh sách tất cả permissions (Admin only)
    /// </summary>
    [HttpGet]
    [HasPermission("permission.view_all")]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _permissionService.GetAllPermissionsAsync();
        return Ok(ApiResponse<IEnumerable<PermissionDto>>.Ok(permissions, "Lấy danh sách permissions thành công"));
    }

    /// <summary>
    /// Lấy permissions nhóm theo module (Admin only)
    /// </summary>
    [HttpGet("grouped")]
    [HasPermission("permission.view_all")]
    public async Task<IActionResult> GetGroupedByModule()
    {
        var grouped = await _permissionService.GetPermissionsGroupedByModuleAsync();
        return Ok(ApiResponse<IEnumerable<PermissionsByModuleDto>>.Ok(grouped, "Lấy permissions theo module thành công"));
    }

    /// <summary>
    /// Lấy permissions theo module (Admin only)
    /// </summary>
    [HttpGet("module/{module}")]
    [HasPermission("permission.view_all")]
    public async Task<IActionResult> GetByModule(string module)
    {
        var permissions = await _permissionService.GetPermissionsByModuleAsync(module);
        return Ok(ApiResponse<IEnumerable<PermissionDto>>.Ok(permissions, "Lấy permissions thành công"));
    }

    /// <summary>
    /// Lấy chi tiết permission theo ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [HasPermission("permission.view_all")]
    public async Task<IActionResult> GetById(int id)
    {
        var permission = await _permissionService.GetPermissionByIdAsync(id);
        if (permission == null)
            return NotFound(ApiResponse<string>.Fail("Không tìm thấy permission", 404));

        return Ok(ApiResponse<PermissionDto>.Ok(permission, "Lấy thông tin permission thành công"));
    }

    /// <summary>
    /// Tạo permission mới (Admin only)
    /// </summary>
    [HttpPost]
    [HasPermission("permission.create")]
    public async Task<IActionResult> Create([FromBody] CreatePermissionDto dto)
    {
        var permission = await _permissionService.CreatePermissionAsync(dto);
        if (permission == null)
            return BadRequest(ApiResponse<string>.Fail("Permission key đã tồn tại", 400));

        return CreatedAtAction(nameof(GetById), new { id = permission.Id }, 
            ApiResponse<PermissionDto>.Ok(permission, "Tạo permission thành công"));
    }

    /// <summary>
    /// Cập nhật permission (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("permission.edit")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermissionDto dto)
    {
        var permission = await _permissionService.UpdatePermissionAsync(id, dto);
        if (permission == null)
            return BadRequest(ApiResponse<string>.Fail("Không tìm thấy permission hoặc key đã tồn tại", 400));

        return Ok(ApiResponse<PermissionDto>.Ok(permission, "Cập nhật permission thành công"));
    }

    /// <summary>
    /// Xóa permission (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("permission.delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _permissionService.DeletePermissionAsync(id);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("Không tìm thấy permission", 404));

        return Ok(ApiResponse<string>.Ok(null, "Xóa permission thành công"));
    }
}
