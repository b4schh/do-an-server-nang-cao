using FootballField.API.Dtos;
using FootballField.API.Dtos.Complex;
using FootballField.API.Services.Interfaces;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballField.API.Controllers
{
    [ApiController]
    [Route("api/complexes")]
    public class ComplexesController : ControllerBase
    {
        private readonly IComplexService _complexService;
        private readonly IUserRepository _userRepository;

        public ComplexesController(IComplexService complexService, IUserRepository userRepository)
        {
            _complexService = complexService;
            _userRepository = userRepository;
        }

        // Lấy tất cả Complexes phân trang
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (complexes, totalCount) = await _complexService.GetPagedComplexesAsync(pageIndex, pageSize);
            var response = new ApiPagedResponse<ComplexDto>(complexes, pageIndex, pageSize, totalCount, "Lấy danh sách sân thành công");
            return Ok(response);
        }

        // Lấy Complex theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var complex = await _complexService.GetComplexByIdAsync(id);
            if (complex == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            return Ok(ApiResponse<ComplexDto>.Ok(complex, "Lấy thông tin sân thành công"));
        }

        // Lấy Complex kèm Fields
        [HttpGet("{id}/with-fields")]
        public async Task<IActionResult> GetWithFields(int id)
        {
            var complex = await _complexService.GetComplexWithFieldsAsync(id);
            if (complex == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            return Ok(ApiResponse<ComplexWithFieldsDto>.Ok(complex, "Lấy thông tin sân thành công"));
        }

        // Lấy Complex kèm Fields và Timeslots đầy đủ với trạng thái availability
        [HttpGet("{id}/full-details")]
        public async Task<IActionResult> GetFullDetails(int id, [FromQuery] DateTime? date = null)
        {
            var targetDate = date ?? DateTime.Today;
            var complex = await _complexService.GetComplexWithFullDetailsAsync(id, targetDate);

            if (complex == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            return Ok(ApiResponse<ComplexFullDetailsDto>.Ok(complex, "Lấy thông tin sân đầy đủ thành công"));
        }

        // Lấy danh sách Complexes của 1 Owner cụ thể
        [HttpGet("admin/owner/{ownerId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetComplexesByOwnerIdForAdmin(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest(ApiResponse<string>.Fail("Owner ID không hợp lệ", 400));

            var owner = await _userRepository.GetUserByIdWithRoleAsync(ownerId);
            if (owner == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy Owner với ID này", 404));

            if (owner.Role != UserRole.Owner && owner.Role != UserRole.Admin)
                return BadRequest(ApiResponse<string>.Fail("User này không phải là Owner hoặc Admin", 400));

            var complexes = await _complexService.GetComplexesByOwnerIdAsync(ownerId);
            return Ok(ApiResponse<IEnumerable<ComplexDto>>.Ok(complexes, "Lấy danh sách sân thành công"));
        }

        // Tìm kiếm sân
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? name = null,
            [FromQuery] string? street = null,
            [FromQuery] string? ward = null,
            [FromQuery] string? province = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] double? minRating = null,
            [FromQuery] double? maxRating = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var complexes = await _complexService.SearchComplexesAsync(
                name, street, ward, province, minPrice, maxPrice, minRating, maxRating);

            var list = complexes?.ToList() ?? new List<ComplexDto>();
            var totalCount = list.Count;
            var paged = list.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var response = new ApiPagedResponse<ComplexDto>(paged, pageIndex, pageSize, totalCount, "Tìm kiếm cụm sân thành công");
            return Ok(response);
        }

        // Lấy danh sách Complexes của mình (Owner chỉ có thể xem Complex của mình)
        [HttpGet("owner/my-complexes")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetMyComplexes()
        {
            // Lấy userId từ JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int ownerId))
                return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực user", 401));

            var complexes = await _complexService.GetComplexesByOwnerIdAsync(ownerId);
            return Ok(ApiResponse<IEnumerable<ComplexDto>>.Ok(complexes, "Lấy danh sách sân thành công"));
        }

        // DEPRECATED: Get complexes by owner ID
        // Use GET /api/complexes/admin/owner/{ownerId} (Admin) or GET /api/complexes/owner/my-complexes (Owner)
        [HttpGet("owner/{ownerId}")]
        [Authorize]
        [Obsolete("Use GET /api/complexes/admin/owner/{ownerId} or GET /api/complexes/owner/my-complexes instead")]
        public async Task<IActionResult> GetByOwnerId(int ownerId)
        {
            var complexes = await _complexService.GetComplexesByOwnerIdAsync(ownerId);
            return Ok(ApiResponse<IEnumerable<ComplexDto>>.Ok(complexes, "Lấy danh sách sân thành công"));
        }

        // Tạo Complex mới (cũ)
        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        [Obsolete("Use POST /api/complexes/owner or POST /api/complexes/admin instead")]
        public async Task<IActionResult> Create([FromBody] CreateComplexDto createComplexDto)
        {
            var created = await _complexService.CreateComplexAsync(createComplexDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ComplexDto>.Ok(created, "Tạo sân thành công", 201));
        }

        // Tạo Complex mới (Owner)
        [HttpPost("owner")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> CreateByOwner([FromBody] CreateComplexByOwnerDto createComplexDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int ownerId))
                return Unauthorized(ApiResponse<string>.Fail("Không thể xác thực user", 401));

            var created = await _complexService.CreateComplexByOwnerAsync(createComplexDto, ownerId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ComplexDto>.Ok(created, "Tạo sân thành công", 201));
        }

        // Tạo Complex mới (Admin)
        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateByAdmin([FromBody] CreateComplexByAdminDto createComplexDto)
        {
            try
            {
                var created = await _complexService.CreateComplexByAdminAsync(createComplexDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ComplexDto>.Ok(created, "Tạo sân thành công", 201));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // Cập nhật Complex
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateComplexDto updateComplexDto)
        {
            var existing = await _complexService.GetComplexByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            await _complexService.UpdateComplexAsync(id, updateComplexDto);
            return Ok(ApiResponse<string>.Ok("", "Cập nhật sân thành công"));
        }

        // Xóa Complex
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _complexService.GetComplexByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            await _complexService.SoftDeleteComplexAsync(id);
            return Ok(ApiResponse<string>.Ok("", "Xóa sân thành công"));
        }

        // Duyệt Complex
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                await _complexService.ApproveComplexAsync(id);
                return Ok(ApiResponse<string>.Ok("", "Phê duyệt sân thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }

        // Từ chối Complex
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                await _complexService.RejectComplexAsync(id);
                return Ok(ApiResponse<string>.Ok("", "Từ chối sân thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message, 400));
            }
        }
    }
}
