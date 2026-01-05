using Microsoft.AspNetCore.Mvc;
using DoAn.Core.Application.DTOs.Base;
using DoAn.Presentation.Api.Middlewares;
using DoAn.Core.Application.Interfaces.SystemConfig;
using DoAn.Core.Application.DTOs.SystemConfig;

namespace FootballField.API.Modules.SystemConfigManagement.Controllers
{
    [ApiController]
    [Route("api/admin/system-configs")]
    [HasPermission("system.manage")]
    public class SystemConfigController : ControllerBase
    {
        private readonly ISystemConfigService _service;

        public SystemConfigController(ISystemConfigService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var configs = await _service.GetAllConfigsAsync();
            return Ok(ApiResponse<IEnumerable<SystemConfigDto>>.Ok(
                configs, 
                "Lấy danh sách cấu hình hệ thống thành công"
            ));
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetByKey(string key)
        {
            var config = await _service.GetConfigByKeyAsync(key);
            if (config == null)
                return NotFound(ApiResponse<object>.Fail(
                    "Không tìm thấy cấu hình", 
                    404
                ));

            return Ok(ApiResponse<SystemConfigDto>.Ok(
                config, 
                "Lấy cấu hình thành công"
            ));
        }

        [HttpPut("{key}")]
        public async Task<IActionResult> Update(string key, UpdateSystemConfigDto dto)
        {
            await _service.UpdateConfigAsync(key, dto);
            return Ok(ApiResponse<object>.Ok(
                null!, 
                "Cập nhật cấu hình thành công"
            ));
        }
    }
}
