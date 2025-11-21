using FootballField.API.Shared.Dtos;
using FootballField.API.Modules.UserManagement.Dtos;
using FootballField.API.Modules.AuthManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FootballField.API.Modules.AuthManagement.Dtos;

namespace FootballField.API.Modules.AuthManagement.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Ok(ApiResponse<List<string>>.Fail(string.Join(", ", errors), 400));
            }

            var result = await _authService.RegisterAsync(request);
            
            if (result == null)
                return Ok(ApiResponse<string>.Fail("Email hoặc số điện thoại đã tồn tại", 400));

            return Ok(ApiResponse<LoginResponse>.Ok(result, "Đăng ký thành công"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Ok(ApiResponse<List<string>>.Fail(string.Join(", ", errors), 400));
            }

            var result = await _authService.LoginAsync(request);
            
            if (result == null)
                return Unauthorized(ApiResponse<string>.Fail("Email hoặc mật khẩu không đúng", 401));

            return Ok(ApiResponse<LoginResponse>.Ok(result, "Đăng nhập thành công"));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Ok(ApiResponse<string>.Fail("Unauthorized", 401));

            var user = await _authService.GetCurrentUserAsync(userId);
            if (user == null)
                return Ok(ApiResponse<string>.Fail("User not found", 404));

            return Ok(ApiResponse<UserDto>.Ok(user, "Lấy thông tin thành công"));
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok(ApiResponse<string>.Ok("Welcome Admin!", "Access granted"));
        }
    }
}
