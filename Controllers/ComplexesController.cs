using FootballField.API.Dtos;
using FootballField.API.Dtos.Complex;
using FootballField.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Controllers
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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (complexes, totalCount) = await _complexService.GetPagedComplexesAsync(pageIndex, pageSize);
            var response = new ApiPagedResponse<ComplexDto>(complexes, pageIndex, pageSize, totalCount, "Lấy danh sách sân thành công");
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var complex = await _complexService.GetComplexByIdAsync(id);
            if (complex == null)
                return Ok(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            return Ok(ApiResponse<ComplexDto>.Ok(complex, "Lấy thông tin sân thành công"));
        }

        [HttpGet("{id}/with-fields")]
        public async Task<IActionResult> GetWithFields(int id)
        {
            var complex = await _complexService.GetComplexWithFieldsAsync(id);
            if (complex == null)
                return Ok(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            return Ok(ApiResponse<ComplexWithFieldsDto>.Ok(complex, "Lấy thông tin sân thành công"));
        }

        [HttpGet("owner/{ownerId}")]
        [Authorize]
        public async Task<IActionResult> GetByOwnerId(int ownerId)
        {
            var complexes = await _complexService.GetComplexesByOwnerIdAsync(ownerId);
            return Ok(ApiResponse<IEnumerable<ComplexDto>>.Ok(complexes, "Lấy danh sách sân thành công"));
        }

      
        [HttpPost]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Create([FromBody] CreateComplexDto createComplexDto)
        {
            var created = await _complexService.CreateComplexAsync(createComplexDto);
            return Ok(ApiResponse<ComplexDto>.Ok(created, "Tạo sân thành công", 201));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateComplexDto updateComplexDto)
        {
            var existing = await _complexService.GetComplexByIdAsync(id);
            if (existing == null)
                return Ok(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            await _complexService.UpdateComplexAsync(id, updateComplexDto);
            return Ok(ApiResponse<string>.Ok(null, "Cập nhật sân thành công"));
        }

       
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _complexService.GetComplexByIdAsync(id);
            if (existing == null)
                return Ok(ApiResponse<string>.Fail("Không tìm thấy sân", 404));

            await _complexService.SoftDeleteComplexAsync(id);
            return Ok(ApiResponse<string>.Ok(null, "Xóa sân thành công"));
        }
    }
}
