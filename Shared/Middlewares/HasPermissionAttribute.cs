using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using FootballField.API.Modules.UserManagement.Services;
using FootballField.API.Shared.Dtos;

namespace FootballField.API.Shared.Middlewares;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permissionKey;

    public HasPermissionAttribute(string permissionKey)
    {
        _permissionKey = permissionKey;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Check if user is authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new ApiResponse<object>
            {
                Success = false,
                Message = "Bạn cần đăng nhập để thực hiện thao tác này",
                Data = null
            });
            return;
        }

        // Get userId from JWT token
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Result = new UnauthorizedObjectResult(new ApiResponse<object>
            {
                Success = false,
                Message = "Token không hợp lệ",
                Data = null
            });
            return;
        }

        // Get permission service from DI container
        var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null)
        {
            context.Result = new ObjectResult(new ApiResponse<object>
            {
                Success = false,
                Message = "Lỗi hệ thống: Permission service không được cấu hình",
                Data = null
            })
            {
                StatusCode = 500
            };
            return;
        }

        // Check if user has permission
        var hasPermission = await permissionService.HasPermissionAsync(userId, _permissionKey);
        if (!hasPermission)
        {
            context.Result = new ObjectResult(new ApiResponse<object>
            {
                Success = false,
                Message = $"Bạn không có quyền thực hiện thao tác này (Yêu cầu: {_permissionKey})",
                Data = null
            })
            {
                StatusCode = 403
            };
            return;
        }
    }
}
