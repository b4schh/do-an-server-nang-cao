using FootballField.API.Dtos;
using FootballField.API.Dtos.TimeSlot;
using FootballField.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Controllers
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var timeSlots = await _timeSlotService.GetAllTimeSlotsAsync();
            return Ok(ApiResponse<IEnumerable<TimeSlotDto>>.Ok(timeSlots, "Lấy danh sách khung giờ thành công"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var timeSlot = await _timeSlotService.GetTimeSlotByIdAsync(id);
            if (timeSlot == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy khung giờ", 404));

            return Ok(ApiResponse<TimeSlotDto>.Ok(timeSlot, "Lấy thông tin khung giờ thành công"));
        }

        [HttpGet("field/{fieldId}")]
        public async Task<IActionResult> GetByFieldId(int fieldId)
        {
            var timeSlots = await _timeSlotService.GetTimeSlotsByFieldIdAsync(fieldId);
            return Ok(ApiResponse<IEnumerable<TimeSlotDto>>.Ok(timeSlots, "Lấy danh sách khung giờ thành công"));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Create([FromBody] CreateTimeSlotDto createTimeSlotDto)
        {
            try
            {
                var created = await _timeSlotService.CreateTimeSlotAsync(createTimeSlotDto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<TimeSlotDto>.Ok(created, "Tạo khung giờ thành công", 201));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTimeSlotDto updateTimeSlotDto)
        {
            try
            {
                await _timeSlotService.UpdateTimeSlotAsync(id, updateTimeSlotDto);
                return Ok(ApiResponse<string>.Ok("", "Cập nhật khung giờ thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")]
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
