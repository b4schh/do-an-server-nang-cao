using FootballField.API.Modules.LocationManagement.Dtos;
using FootballField.API.Modules.LocationManagement.Services;
using FootballField.API.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Modules.LocationManagement.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationController : ControllerBase
{
    private readonly IProvinceService _provinceService;
    private readonly IWardService _wardService;

    public LocationController(IProvinceService provinceService, IWardService wardService)
    {
        _provinceService = provinceService;
        _wardService = wardService;
    }

    /// <summary>
    /// Lấy danh sách tất cả tỉnh/thành phố
    /// </summary>
    /// <returns>Danh sách tỉnh/thành phố</returns>
    [HttpGet("provinces")]
    public async Task<IActionResult> GetAllProvinces()
    {
        var provinces = await _provinceService.GetAllProvincesAsync();
        return Ok(ApiResponse<IEnumerable<ProvinceDto>>.Ok(provinces, "Lấy danh sách tỉnh/thành phố thành công"));
    }

    /// <summary>
    /// Lấy thông tin tỉnh/thành phố theo mã
    /// </summary>
    /// <param name="code">Mã tỉnh/thành phố</param>
    /// <returns>Thông tin tỉnh/thành phố</returns>
    [HttpGet("provinces/{code}")]
    public async Task<IActionResult> GetProvinceByCode(int code)
    {
        var province = await _provinceService.GetProvinceByCodeAsync(code);
        if (province == null)
            return NotFound(ApiResponse<string>.Fail("Không tìm thấy tỉnh/thành phố", 404));

        return Ok(ApiResponse<ProvinceDto>.Ok(province, "Lấy thông tin tỉnh/thành phố thành công"));
    }

    /// <summary>
    /// Lấy danh sách phường/xã theo mã tỉnh/thành phố
    /// </summary>
    /// <param name="code">Mã tỉnh/thành phố</param>
    /// <returns>Danh sách phường/xã</returns>
    [HttpGet("provinces/{code}/wards")]
    public async Task<IActionResult> GetWardsByProvinceCode(int code)
    {
        var wards = await _wardService.GetWardsByProvinceCodeAsync(code);
        return Ok(ApiResponse<IEnumerable<WardDto>>.Ok(wards, "Lấy danh sách phường/xã thành công"));
    }

    /// <summary>
    /// Lấy thông tin phường/xã theo mã
    /// </summary>
    /// <param name="code">Mã phường/xã</param>
    /// <returns>Thông tin phường/xã</returns>
    [HttpGet("wards/{code}")]
    public async Task<IActionResult> GetWardByCode(int code)
    {
        var ward = await _wardService.GetWardByCodeAsync(code);
        if (ward == null)
            return NotFound(ApiResponse<string>.Fail("Không tìm thấy phường/xã", 404));

        return Ok(ApiResponse<WardDto>.Ok(ward, "Lấy thông tin phường/xã thành công"));
    }
}
