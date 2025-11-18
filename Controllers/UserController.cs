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

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
    [Authorize(Policy = "RequireCustomerRole")]
    public async Task<IActionResult> UploadAvatar(int id, IFormFile file)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn file để upload!"));
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(ApiResponse<object>.Fail("Chỉ chấp nhận file ảnh (JPEG, PNG, WEBP)!"));
            }

            // Validate file size (2MB for avatar)
            if (file.Length > 2 * 1024 * 1024)
            {
                return BadRequest(ApiResponse<object>.Fail("Kích thước file không được vượt quá 2MB!"));
            }

            // Check if user exists and has permission
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value 
                           ?? User.FindFirst("email")?.Value 
                           ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(ApiResponse<object>.Fail("Không thể xác thực người dùng!"));
            }

            var currentUser = await _userService.GetUserByEmailAsync(userEmail);
            if (currentUser.Id != id)
            {
                return Forbid("Bạn chỉ có thể upload avatar cho chính mình!");
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"avatar-{id}-{Guid.NewGuid()}{fileExtension}";
            var objectName = $"avatars/{fileName}";

            // Delete old avatar if exists
            if (!string.IsNullOrEmpty(currentUser.AvatarUrl))
            {
                try
                {
                    var uri = new Uri(currentUser.AvatarUrl);
                    var oldObjectName = uri.AbsolutePath.TrimStart('/');
                    // Remove bucket name from path
                    var bucketIndex = oldObjectName.IndexOf('/');
                    if (bucketIndex > 0)
                    {
                        oldObjectName = oldObjectName.Substring(bucketIndex + 1);
                    }
                    await _storageService.DeleteAsync(oldObjectName);
                }
                catch
                {
                    // Ignore errors when deleting old avatar
                }
            }

            // Upload to MinIO
            string avatarUrl;
            using (var stream = file.OpenReadStream())
            {
                avatarUrl = await _storageService.UploadAsync(stream, objectName, file.ContentType);
            }

            // Update user avatar URL in database
            var result = await _userService.UpdateAvatarAsync(id, avatarUrl);

            return Ok(ApiResponse<UserResponseDto>.Ok(result, "Upload avatar thành công!"));
        }
        catch (Exception)
        {
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy người dùng!"));
        }
     
    }

    [HttpDelete("{id:int}/avatar")]
    [Authorize(Policy = "RequireCustomerRole")]
    public async Task<IActionResult> DeleteAvatar(int id)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value 
                           ?? User.FindFirst("email")?.Value 
                           ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(ApiResponse<object>.Fail("Không thể xác thực người dùng!"));
            }

            var currentUser = await _userService.GetUserByEmailAsync(userEmail);
            if (currentUser.Id != id)
            {
                return Forbid("Bạn chỉ có thể xóa avatar của chính mình!");
            }

            // Delete from MinIO if exists
            if (!string.IsNullOrEmpty(currentUser.AvatarUrl))
            {
                try
                {
                    var uri = new Uri(currentUser.AvatarUrl);
                    var objectName = uri.AbsolutePath.TrimStart('/');
                    var bucketIndex = objectName.IndexOf('/');
                    if (bucketIndex > 0)
                    {
                        objectName = objectName.Substring(bucketIndex + 1);
                    }
                    await _storageService.DeleteAsync(objectName);
                }
                catch
                {
                    // Ignore errors when deleting from storage
                }
            }

            // Remove avatar URL from database
            var result = await _userService.UpdateAvatarAsync(id, null);

            return Ok(ApiResponse<UserResponseDto>.Ok(result, "Xóa avatar thành công!"));
        }
        catch (Exception)
        {
            return NotFound(ApiResponse<object>.Fail("Không tìm thấy người dùng!"));
        }
 
    }
    


[HttpPatch("{id:int}/profile")]
[Authorize(Policy = "RequireCustomerRole")] // hoặc [Authorize] nếu bạn dùng roles khác
public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserProfileDto dto)
{
    // Lấy email từ claim (cùng cách như UploadAvatar)
    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    if (string.IsNullOrEmpty(userEmail))
        return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực người dùng"));

    // Lấy user hiện tại theo email
    var currentUser = await _userService.GetUserByEmailAsync(userEmail);
    if (currentUser == null)
        return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực người dùng"));

    if (currentUser.Id != id)
        return BadRequest(ApiResponse<string>.Fail("Bạn chỉ có thể cập nhật profile của chính mình"));

    try
    {
        var updated = await _userService.UpdateUserProfileAsync(id, dto);
        return Ok(ApiResponse<UserResponseDto>.Ok(updated, "Cập nhật thông tin thành công"));
    }
    catch (Exception ex)
    {
        // Trả lỗi rõ ràng (có thể thay bằng BadRequest tuỳ case)
        return BadRequest(ApiResponse<string>.Fail(ex.Message));
    }
}


 [HttpPost("{id:int}/change-password")]
    [Authorize(Policy = "RequireCustomerRole")]
    public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto dto)
    {
        try
        {
            // Kiểm tra xem user có quyền đổi password của chính mình không
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value 
                           ?? User.FindFirst("email")?.Value 
                           ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new ApiResponse<object>(false, "Không thể xác thực người dùng!", null));
            }

            var currentUser = await _userService.GetUserByEmailAsync(userEmail);
            if (currentUser.Id != id)
            {
                return Forbid();
            }

            var result = await _userService.ChangePasswordAsync(id, dto.CurrentPassword, dto.NewPassword);
            
            if (result)
            {
                return Ok(new ApiResponse<object>(true, "Đổi mật khẩu thành công!", null));
            }
            else
            {
                return BadRequest(new ApiResponse<object>(false, "Mật khẩu hiện tại không đúng!", null));
            }
        }
        catch (Exception)
        {
            return NotFound(new ApiResponse<object>(false, "Không tìm thấy người dùng!", null));
        }

    }
    }
}