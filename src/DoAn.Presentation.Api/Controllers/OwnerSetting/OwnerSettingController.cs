using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DoAn.Core.Application.DTOs.Base;
using DoAn.Presentation.Api.Middlewares;
using System.Security.Claims;
using DoAn.Core.Application.Interfaces.OwnerSetting;
using DoAn.Core.Application.DTOs.OwnerSetting;

namespace FootballField.API.Modules.OwnerSettingsManagement.Controllers
{
    [ApiController]
    [Route("api/owner/settings")]
    [Authorize]
    public class OwnerSettingController : ControllerBase
    {
        private readonly IOwnerSettingService _service;

        public OwnerSettingController(IOwnerSettingService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get owner settings with system defaults
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdClaim) || !int.TryParse(ownerIdClaim, out int ownerId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid user", 401));

            var result = await _service.GetSettingsWithDefaultsAsync(ownerId);
            return Ok(ApiResponse<OwnerSettingResponseDto>.Ok(
                result,
                "Lấy cài đặt thành công"
            ));
        }

        /// <summary>
        /// Update owner settings (lazy creation)
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateSettings(UpdateOwnerSettingDto dto)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdClaim) || !int.TryParse(ownerIdClaim, out int ownerId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid user", 401));

            await _service.UpdateSettingsAsync(ownerId, dto);
            return Ok(ApiResponse<object>.Ok(
                null!,
                "Cập nhật cài đặt thành công"
            ));
        }

        /// <summary>
        /// Update bank information with QR code upload
        /// </summary>
        [HttpPut("bank-info")]
        public async Task<IActionResult> UpdateBankInfo([FromForm] UpdateBankInfoDto dto)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdClaim) || !int.TryParse(ownerIdClaim, out int ownerId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid user", 401));

            await _service.UpdateBankInfoAsync(ownerId, dto);
            return Ok(ApiResponse<object>.Ok(
                null!,
                "Cập nhật thông tin ngân hàng thành công"
            ));
        }

        /// <summary>
        /// Validate if owner has complete bank info
        /// </summary>
        [HttpGet("bank-info/validate")]
        public async Task<IActionResult> ValidateBankInfo()
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdClaim) || !int.TryParse(ownerIdClaim, out int ownerId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid user", 401));

            var isValid = await _service.ValidateBankInfoAsync(ownerId);
            return Ok(ApiResponse<bool>.Ok(
                isValid,
                isValid ? "Thông tin ngân hàng đầy đủ" : "Thông tin ngân hàng chưa đầy đủ"
            ));
        }
    }
}