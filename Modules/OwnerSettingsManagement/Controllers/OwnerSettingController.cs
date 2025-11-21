using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FootballField.API.Modules.OwnerSettingsManagement.Services;
using FootballField.API.Modules.OwnerSettingsManagement.Dtos;

namespace FootballField.API.Modules.OwnerSettingsManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class OwnerSettingController : ControllerBase
    {
        private readonly IOwnerSettingService _service;

        public OwnerSettingController(IOwnerSettingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOwnerSettingDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateOwnerSettingDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}