using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DoAn.Core.Application.DTOs.Base;
using DoAn.Presentation.Api.Middlewares;
using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Core.Application.DTOs.Complex;

namespace FootballField.API.Modules.ComplexManagement.Controllers
{
    [ApiController]
    [Route("api/complexes")]
    public class ComplexesController : ControllerBase
    {
        private readonly IComplexService _complexService;

        public ComplexesController(IComplexService complexService)
        {
            _complexService = complexService;
        }

        // Lấy tất cả Complexes phân trang (chỉ trả về approved complexes cho public)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (complexes, totalCount) = await _complexService.GetPagedComplexesAsync(pageIndex, pageSize);
            var response = new ApiPagedResponse<ComplexDto>(complexes, pageIndex, pageSize, totalCount, "Lấy danh sách sân thành công");
            return Ok(response);
        }

        // Lấy tất cả Complexes cho Admin (bao gồm cả pending, rejected) - Admin only
        [HttpGet("admin/all")]
        [HasPermission("complex.view_all")]
        public async Task<IActionResult> GetAllForAdmin(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] int? status = null)
        {
            var (complexes, totalCount) = await _complexService.GetAllComplexesForAdminAsync(pageIndex, pageSize);
            
            // Filter by status if provided (0=Pending, 1=Approved, 2=Rejected)
            if (status.HasValue)
            {
                complexes = complexes.Where(c => (int)c.Status == status.Value);
                totalCount = complexes.Count();
            }
            
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

        // Admin only - Lấy Complex detail với rating, review count, images
        [HttpGet("{id}/admin-details")]
        [HasPermission("complex.view_all")]
        public async Task<IActionResult> GetAdminDetails(int id)
        {
            var complex = await _complexService.GetComplexDetailForAdminAsync(id);
            if (complex == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            return Ok(ApiResponse<AdminComplexDetailDto>.Ok(complex, "Lấy thông tin sân thành công"));
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

        // Lấy Complex kèm Fields và Timeslots với availability theo từng ngày trong tuần
        [HttpGet("{id}/weekly-details")]
        public async Task<IActionResult> GetWeeklyDetails(
            int id, 
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            // Mặc định lấy 7 ngày từ hôm nay
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? DateTime.Today.AddDays(6);

            // Validate date range
            if (end < start)
                return BadRequest(ApiResponse<string>.Fail("endDate phải sau startDate", 400));

            if ((end - start).TotalDays > 30)
                return BadRequest(ApiResponse<string>.Fail("Chỉ cho phép lấy tối đa 30 ngày", 400));

            var complex = await _complexService.GetComplexWeeklyDetailsAsync(id, start, end);

            if (complex == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            return Ok(ApiResponse<ComplexWeeklyDetailsDto>.Ok(complex, "Lấy thông tin sân theo tuần thành công"));
        }

        // Lấy availability của complex theo từng ngày
        [HttpGet("{id}/availability")]
        public async Task<IActionResult> GetAvailability(
            int id,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] int days = 7)
        {
            // Mặc định lấy từ hôm nay
            var start = startDate ?? DateOnly.FromDateTime(DateTime.Today);

            // Validate days
            if (days < 1 || days > 30)
                return BadRequest(ApiResponse<string>.Fail("days phải trong khoảng 1-30", 400));

            var availability = await _complexService.GetAvailabilityAsync(id, start, days);

            if (availability == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            return Ok(ApiResponse<AvailabilityDto>.Ok(availability, "Lấy thông tin availability thành công"));
        }

        // Lấy danh sách Complexes của 1 Owner cụ thể
        [HttpGet("admin/owner/{ownerId}")]
        [HasPermission("complex.view_all")]
        public async Task<IActionResult> GetComplexesByOwnerIdForAdmin(int ownerId)
        {
            if (ownerId <= 0)
                return BadRequest(ApiResponse<string>.Fail("Owner ID không hợp lệ", 400));

            var isValidOwner = await _complexService.ValidateOwnerRoleAsync(ownerId);
            if (!isValidOwner)
                return BadRequest(ApiResponse<string>.Fail("Không tìm thấy Owner với ID này hoặc user không phải là Owner/Admin", 400));

            var complexes = await _complexService.GetComplexesByOwnerIdAsync(ownerId);
            return Ok(ApiResponse<IEnumerable<ComplexDto>>.Ok(complexes, "Lấy danh sách sân thành công"));
        }

        // Tìm kiếm sân
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? name = null,
            [FromQuery] string? ward = null,
            [FromQuery] string? province = null,
            [FromQuery] string? surfaceType = null,
            [FromQuery] string? fieldSize = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] double? minRating = null,
            [FromQuery] double? maxRating = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var complexes = await _complexService.SearchComplexesAsync(
                name, ward, province, surfaceType, fieldSize, minPrice, maxPrice, minRating, maxRating);

            var list = complexes?.ToList() ?? new List<ComplexDto>();
            var totalCount = list.Count;
            var paged = list.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var response = new ApiPagedResponse<ComplexDto>(paged, pageIndex, pageSize, totalCount, "Tìm kiếm cụm sân thành công");
            return Ok(response);
        }

        // Lấy danh sách Complexes của mình (Owner chỉ có thể xem Complex của mình)
        [HttpGet("owner/my-complexes")]
        [HasPermission("complex.edit_own")]
        public async Task<IActionResult> GetMyComplexes([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var ownerId = GetUserId();
            var (complexes, totalCount) = await _complexService.GetComplexesByOwnerIdPagedAsync(ownerId, pageIndex, pageSize);
            var response = new ApiPagedResponse<ComplexDto>(complexes, pageIndex, pageSize, totalCount, "Lấy danh sách sân thành công");
            return Ok(response);
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
        [HasPermission("complex.create")]
        [Obsolete("Use POST /api/complexes/owner or POST /api/complexes/admin instead")]
        public async Task<IActionResult> Create([FromBody] CreateComplexDto createComplexDto)
        {
            var created = await _complexService.CreateComplexAsync(createComplexDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ComplexDto>.Ok(created, "Tạo sân thành công", 201));
        }

        // Tạo Complex mới (Owner)
        [HttpPost("owner")]
        [HasPermission("complex.create")]
        public async Task<IActionResult> CreateByOwner([FromBody] CreateComplexByOwnerDto createComplexDto)
        {
            var ownerId = GetUserId();
            var created = await _complexService.CreateComplexByOwnerAsync(createComplexDto, ownerId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ComplexDto>.Ok(created, "Tạo sân thành công", 201));
        }

        // Tạo Complex mới (Admin)
        [HttpPost("admin")]
        [HasPermission("complex.approve")]
        public async Task<IActionResult> CreateByAdmin([FromBody] CreateComplexByAdminDto createComplexDto)
        {
            var created = await _complexService.CreateComplexByAdminAsync(createComplexDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ComplexDto>.Ok(created, "Tạo sân thành công", 201));
        }

        // Cập nhật Complex
        [HttpPut("{id}")]
        [HasPermission("complex.edit_own")]
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
        [HasPermission("complex.delete_own")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _complexService.GetComplexByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            await _complexService.SoftDeleteComplexAsync(id);
            return Ok(ApiResponse<string>.Ok("", "Xóa sân thành công"));
        }

        // Bật/tắt trạng thái hoạt động của Complex
        [HttpPatch("{id}/toggle-active")]
        [HasPermission("complex.edit_own")]
        public async Task<IActionResult> ToggleActive(int id, [FromBody] bool isActive)
        {
            var existing = await _complexService.GetComplexByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            var result = await _complexService.ToggleActiveAsync(id, isActive);
            if (!result)
                return BadRequest(ApiResponse<string>.Fail("Cập nhật trạng thái hoạt động thất bại", 400));

            return Ok(ApiResponse<string>.Ok("", "Cập nhật trạng thái hoạt động thành công"));
        }

        // Duyệt Complex
        [HttpPatch("{id}/approve")]
        [HasPermission("complex.approve")]
        public async Task<IActionResult> Approve(int id)
        {
            await _complexService.ApproveComplexAsync(id);
            return Ok(ApiResponse<string>.Ok("", "Phê duyệt sân thành công"));
        }

        // Từ chối Complex
        [HttpPatch("{id}/reject")]
        [HasPermission("complex.approve")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectComplexDto dto)
        {
            await _complexService.RejectComplexAsync(id, dto.Reason);
            return Ok(ApiResponse<string>.Ok("", "Từ chối sân thành công"));
        }

        [HttpPatch("{id}/resubmit")]
        [HasPermission("complex.edit_own")]
        public async Task<IActionResult> Resubmit(int id)
        {
            var ownerId = GetUserId();
            var complex = await _complexService.GetComplexByIdAsync(id);
            
            if (complex == null)
                throw new KeyNotFoundException("Không tìm thấy cụm sân");
            
            if (complex.OwnerId != ownerId)
                throw new UnauthorizedAccessException("Bạn không có quyền gửi lại yêu cầu phê duyệt cho cụm sân này");

            await _complexService.ResubmitComplexAsync(id);
            return Ok(ApiResponse<string>.Ok("", "Đã gửi lại yêu cầu phê duyệt thành công"));
        }

        // Bulk Setup: Create complex with fields and timeslots
        [HttpPost("owner/bulk-setup")]
        [HasPermission("complex.create")]
        public async Task<IActionResult> BulkSetup([FromBody] BulkSetupComplexDto bulkSetupDto)
        {
            var ownerId = GetUserId();
            var created = await _complexService.BulkSetupComplexAsync(bulkSetupDto, ownerId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, 
                ApiResponse<ComplexDto>.Ok(created, $"Tạo thành công cụm sân với {bulkSetupDto.Fields.Count} sân", 201));
        }

        // Helper method to get userId from JWT claims
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("Không thể xác thực user");
            return userId;
        }
    }
}
