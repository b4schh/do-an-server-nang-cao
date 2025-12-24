
using FootballField.API.Shared.Dtos;
using FootballField.API.Modules.FieldManagement.Dtos;
using FootballField.API.Modules.FieldManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FootballField.API.Shared.Middlewares;

namespace FootballField.API.Modules.FieldManagement.Controllers
{
    [ApiController]
    [Route("api/timeslots")]
    public class TimeSlotsController : ControllerBase
    {
        private readonly ITimeSlotService _timeSlotService;

        public TimeSlotsController(ITimeSlotService timeSlotService)
        {
            _timeSlotService = timeSlotService;
        }

        // Lấy tất cả TimeSlots
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var timeSlots = await _timeSlotService.GetAllTimeSlotsAsync();
            return Ok(ApiResponse<IEnumerable<TimeSlotDto>>.Ok(timeSlots, "Lấy danh sách khung giờ thành công"));
        }

        // Lấy TimeSlot theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var timeSlot = await _timeSlotService.GetTimeSlotByIdAsync(id);
            if (timeSlot == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy khung giờ", 404));

            return Ok(ApiResponse<TimeSlotDto>.Ok(timeSlot, "Lấy thông tin khung giờ thành công"));
        }

        // Lấy TimeSlot theo FieldID
        [HttpGet("field/{fieldId}")]
        public async Task<IActionResult> GetByFieldId(int fieldId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (timeSlots, totalCount) = await _timeSlotService.GetTimeSlotsByFieldIdPagedAsync(fieldId, pageIndex, pageSize);
            var response = new ApiPagedResponse<TimeSlotDto>(timeSlots, pageIndex, pageSize, totalCount, "Lấy danh sách khung giờ thành công");
            return Ok(response);
        }

        // Lấy danh sách TimeSlots của mình (Owner)
        [HttpGet("owner/my-time-slots")]
        [HasPermission("timeslot.edit_own")]
        public async Task<IActionResult> GetMyTimeSlots(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? complexId = null,
            [FromQuery] int? fieldId = null,
            [FromQuery] bool? isActive = null)
        {
            var ownerId = GetUserId();
            var (timeSlots, totalCount) = await _timeSlotService.GetTimeSlotsByOwnerIdPagedAsync(
                ownerId, pageIndex, pageSize, searchTerm, complexId, fieldId, isActive);
            var response = new ApiPagedResponse<TimeSlotDto>(timeSlots, pageIndex, pageSize, totalCount, "Lấy danh sách khung giờ thành công");
            return Ok(response);
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // Tạo TimeSlot mới
        [HttpPost]
        [HasPermission("timeslot.create")]
        public async Task<IActionResult> Create([FromBody] CreateTimeSlotDto createTimeSlotDto)
        {
            var result = await _timeSlotService.CreateTimeSlotAsync(createTimeSlotDto);

            if (!result.isSuccess)
            {
                // Trả lỗi 400 (BadRequest)
                return BadRequest(ApiResponse<string>.Fail(result.errorMessage!, 400));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.data!.Id },
                ApiResponse<TimeSlotDto>.Ok(result.data, "Tạo khung giờ thành công", 201)
            );
        }

        // Cập nhật TimeSlot
        [HttpPut("{id}")]
        [HasPermission("timeslot.edit_own")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTimeSlotDto updateTimeSlotDto)
        {
            var result = await _timeSlotService.UpdateTimeSlotAsync(id, updateTimeSlotDto);

            if (!result.isSuccess)
                return BadRequest(ApiResponse<string>.Fail(result.errorMessage!, 400));

            return Ok(ApiResponse<string>.Ok("", "Cập nhật khung giờ thành công"));
        }

        // Toggle trạng thái hoạt động của TimeSlot
        [HttpPatch("{id}/toggle-active")]
        [HasPermission("timeslot.edit_own")]
        public async Task<IActionResult> ToggleActive(int id, [FromBody] bool isActive)
        {
            var result = await _timeSlotService.ToggleActiveAsync(id, isActive);
            if (!result.isSuccess)
                return BadRequest(ApiResponse<string>.Fail(result.errorMessage ?? "Cập nhật trạng thái thất bại"));
            return Ok(ApiResponse<string>.Ok(null, "Cập nhật trạng thái thành công"));
        }
        
        // Xóa TimeSlot
        [HttpDelete("{id}")]
        [HasPermission("timeslot.delete_own")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _timeSlotService.GetTimeSlotByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy khung giờ", 404));

            await _timeSlotService.DeleteTimeSlotAsync(id);
            return Ok(ApiResponse<string>.Ok("", "Xóa khung giờ thành công"));
        }
    }
}
