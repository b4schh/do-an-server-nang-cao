using FootballField.API.Dtos;
using FootballField.API.Dtos.User;
using FootballField.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using FootballField.API.Storage;

namespace FootballField.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IUserService _userService;

        public UsersController(IUserService userService, IStorageService storageService)
        {
            _userService = userService;
            _storageService = storageService;
        }

        /// Lấy danh sách người dùng với phân trang
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (users, totalCount) = await _userService.GetPagedUsersAsync(pageIndex, pageSize);
            var response = new ApiPagedResponse<UserDto>(users, pageIndex, pageSize, totalCount, "Lấy danh sách người dùng thành công");
            return Ok(response);
        }

        /// Lấy thông tin người dùng theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng", 404));

            return Ok(ApiResponse<UserDto>.Ok(user, "Lấy thông tin người dùng thành công"));
        }

        /// Tạo người dùng mới
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
        {
            if (await _userService.EmailExistsAsync(createUserDto.Email))
                return Ok(ApiResponse<string>.Fail("Email đã tồn tại", 400));

            var created = await _userService.CreateUserAsync(createUserDto);
            return Ok(ApiResponse<UserDto>.Ok(created, "Tạo người dùng thành công", 201));
        }

        /// Cập nhật thông tin người dùng
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            var existing = await _userService.GetUserByIdAsync(id);
            if (existing == null)
                return Ok(ApiResponse<string>.Fail("Không tìm thấy người dùng", 404));

            await _userService.UpdateUserAsync(id, updateUserDto);
            return Ok(ApiResponse<string>.Ok("", "Cập nhật người dùng thành công"));
        }

        // Cập nhật role của User
        [HttpPatch("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDto updateUserRoleDto)
        {
            var existing = await _userService.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng", 404));

            await _userService.UpdateUserRoleAsync(id, updateUserRoleDto);
            return Ok(ApiResponse<string>.Ok("", "Cập nhật role người dùng thành công"));
        }

        /// Xóa người dùng (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _userService.GetUserByIdAsync(id);
            if (existing == null)
                return Ok(ApiResponse<string>.Fail("Không tìm thấy người dùng", 404));

            await _userService.SoftDeleteUserAsync(id);
            return Ok(ApiResponse<string>.Ok("", "Xóa người dùng thành công"));
        }

        [HttpPost("{id:int}/upload-avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(int id, IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<string>.Fail("Vui lòng chọn file để upload", 400));
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(ApiResponse<string>.Fail("Định dạng file không hợp lệ. Chỉ chấp nhận ảnh (JPEG, PNG, WEBP)", 400));
                }

                // Validate file size (2MB)
                if (file.Length > 2 * 1024 * 1024)
                {
                    return BadRequest(ApiResponse<string>.Fail("Kích thước file không được vượt quá 2MB", 400));
                }

                // Get current user ID from JWT
                var userId = GetCurrentUserId();
                if (userId != id)
                {
                    return StatusCode(403, ApiResponse<string>.Fail("Bạn chỉ có thể upload avatar cho chính mình", 403));
                }

                var currentUser = await _userService.GetUserByIdAsync(id);
                if (currentUser == null)
                {
                    return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng", 404));
                }

                // Delete old avatar if exists (before upload new one)
                if (!string.IsNullOrEmpty(currentUser.AvatarUrl))
                {
                    await DeleteOldAvatar(currentUser.AvatarUrl);
                }

                // Generate unique filename with avatars/ prefix
                var fileExtension = Path.GetExtension(file.FileName ?? "file.jpg");
                var fileName = $"avatar-{id}-{Guid.NewGuid()}{fileExtension}";
                var objectName = $"avatars/{fileName}";

                // Upload to MinIO
                string avatarRelativePath;
                using (var stream = file.OpenReadStream())
                {
                    avatarRelativePath = await _storageService.UploadAsync(stream, objectName, file.ContentType);
                }

                // Update user avatar URL in database (save relative path)
                var result = await _userService.UpdateAvatarAsync(id, avatarRelativePath);

                return Ok(ApiResponse<UserResponseDto>.Ok(result, "Upload avatar thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(401, ApiResponse<string>.Fail(ex.Message, 401));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi khi upload avatar: {ex.Message}", 500));
            }
        }

        [HttpDelete("{id:int}/avatar")]
        [Authorize]
        public async Task<IActionResult> DeleteAvatar(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId != id)
                {
                    return StatusCode(403, ApiResponse<string>.Fail("Bạn chỉ có thể xóa avatar của chính mình", 403));
                }

                var currentUser = await _userService.GetUserByIdAsync(id);
                if (currentUser == null)
                {
                    return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng", 404));
                }

                // Delete from MinIO if exists
                if (!string.IsNullOrEmpty(currentUser.AvatarUrl))
                {
                    await DeleteOldAvatar(currentUser.AvatarUrl);
                }

                // Remove avatar URL from database
                var result = await _userService.UpdateAvatarAsync(id, null);

                return Ok(ApiResponse<UserResponseDto>.Ok(result, "Xóa avatar thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"Lỗi khi xóa avatar: {ex.Message}", 500));
            }
        }


        [HttpPatch("{id:int}/profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId != id)
                {
                    return StatusCode(403, ApiResponse<string>.Fail("Bạn chỉ có thể cập nhật thông tin của chính mình", 403));
                }

                var updated = await _userService.UpdateUserProfileAsync(id, dto);
                return Ok(ApiResponse<UserResponseDto>.Ok(updated, "Cập nhật thông tin thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }


        [HttpPost("{id:int}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId != id)
                {
                    return StatusCode(403, ApiResponse<string>.Fail("Bạn chỉ có thể đổi mật khẩu của chính mình", 403));
                }

                var result = await _userService.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);

                if (result)
                {
                    return Ok(ApiResponse<string>.Ok(null, "Đổi mật khẩu thành công"));
                }
                else
                {
                    return BadRequest(ApiResponse<string>.Fail("Mật khẩu hiện tại không đúng", 400));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Không thể xác thực người dùng");
            }
            return userId;
        }

        private async Task DeleteOldAvatar(string avatarUrl)
        {
            try
            {
                // avatarUrl có thể là relative path (/{bucket}/avatars/...) hoặc full URL
                string objectName;

                if (avatarUrl.StartsWith("http"))
                {
                    // Nếu là full URL, extract object name
                    var uri = new Uri(avatarUrl);
                    objectName = uri.AbsolutePath.TrimStart('/');
                    // Remove bucket name from path
                    var bucketIndex = objectName.IndexOf('/');
                    if (bucketIndex > 0)
                    {
                        objectName = objectName.Substring(bucketIndex + 1);
                    }
                }
                else
                {
                    // Nếu là relative path, remove /{bucket}/ prefix
                    objectName = avatarUrl.TrimStart('/');
                    var parts = objectName.Split('/', 2);
                    if (parts.Length > 1)
                    {
                        objectName = parts[1]; // Lấy phần sau bucket name
                    }
                }

                await _storageService.DeleteAsync(objectName);
            }
            catch
            {
                // Ignore errors when deleting old avatar
            }
        }
    }
}