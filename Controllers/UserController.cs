using FootballField.API.Dtos;
using FootballField.API.Dtos.User;
using FootballField.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
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
    }
}