using FootballField.API.Dtos;
using FootballField.API.Dtos.Field;
using FootballField.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Controllers
{
    [ApiController]
    [Route("api/fields")]
    public class FieldsController : ControllerBase
    {
        private readonly IFieldService _fieldService;

        public FieldsController(IFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        // Lấy tất cả Fields phân trang
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (fields, totalCount) = await _fieldService.GetPagedFieldsAsync(pageIndex, pageSize);
            var response = new ApiPagedResponse<FieldDto>(fields, pageIndex, pageSize, totalCount, "Lấy danh sách sân con thành công");
            return Ok(response);
        }


        // Lấy Field theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var field = await _fieldService.GetFieldByIdAsync(id);
            if (field == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân con", 404));

            return Ok(ApiResponse<FieldDto>.Ok(field, "Lấy thông tin sân con thành công"));
        }

        // Lấy Field kèm Timeslots
        [HttpGet("{id}/with-timeslots")]
        public async Task<IActionResult> GetWithTimeSlots(int id)
        {
            var field = await _fieldService.GetFieldWithTimeSlotsAsync(id);
            if (field == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân con", 404));

            return Ok(ApiResponse<FieldWithTimeSlotsDto>.Ok(field, "Lấy thông tin sân con thành công"));
        }


        // Lấy Field theo ComplexID
        [HttpGet("complex/{complexId}")]
        public async Task<IActionResult> GetByComplexId(int complexId)
        {
            var fields = await _fieldService.GetFieldsByComplexIdAsync(complexId);
            return Ok(ApiResponse<IEnumerable<FieldDto>>.Ok(fields, "Lấy danh sách sân con thành công"));
        }


        // Tạo Field mới
        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Create([FromBody] CreateFieldDto createFieldDto)
        {
            var created = await _fieldService.CreateFieldAsync(createFieldDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<FieldDto>.Ok(created, "Tạo sân con thành công", 201));
        }


        /// Cập nhật Field
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateFieldDto updateFieldDto)
        {
            var existing = await _fieldService.GetFieldByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân con", 404));

            await _fieldService.UpdateFieldAsync(id, updateFieldDto);
            return Ok(ApiResponse<string>.Ok("", "Cập nhật sân con thành công"));
        }


        // Xóa Field
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _fieldService.GetFieldByIdAsync(id);
            if (existing == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy sân con", 404));

            await _fieldService.SoftDeleteFieldAsync(id);
            return Ok(ApiResponse<string>.Ok("", "Xóa sân con thành công"));
        }
    }
}
