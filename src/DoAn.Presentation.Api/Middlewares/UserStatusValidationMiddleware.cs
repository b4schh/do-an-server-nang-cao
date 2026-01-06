using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using DoAn.Core.Application.Interfaces.User;
using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.DTOs.Base;

namespace DoAn.Presentation.Api.Middlewares;

public class UserStatusValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserStatusValidationMiddleware> _logger;

    public UserStatusValidationMiddleware(RequestDelegate next, ILogger<UserStatusValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        // Skip validation for anonymous endpoints
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Skip validation for certain paths (auth, refresh-token, etc.)
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/auth/login") || 
            path.Contains("/auth/register") || 
            path.Contains("/auth/refresh-token") ||
            path.Contains("/swagger"))
        {
            await _next(context);
            return;
        }

        // Extract userId from JWT claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            await _next(context);
            return;
        }

        // Validate user status
        var user = await userRepository.GetByIdAsync(userId);
        
        if (user == null || user.IsDeleted)
        {
            _logger.LogWarning("User {UserId} attempted to access system but account is deleted", userId);
            await WriteErrorResponse(context, "Tài khoản của bạn đã bị xóa khỏi hệ thống", 403);
            return;
        }

        if (user.Status == UserStatus.Banned)
        {
            _logger.LogWarning("User {UserId} attempted to access system but account is banned", userId);
            await WriteErrorResponse(context, "Tài khoản của bạn đã bị khóa", 403);
            return;
        }

        if (user.Status == UserStatus.Inactive)
        {
            _logger.LogWarning("User {UserId} attempted to access system but account is inactive", userId);
            await WriteErrorResponse(context, "Tài khoản của bạn chưa được kích hoạt", 403);
            return;
        }

        // User is valid, continue
        await _next(context);
    }

    private static async Task WriteErrorResponse(HttpContext context, string message, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<string>.Fail(message, statusCode);
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
