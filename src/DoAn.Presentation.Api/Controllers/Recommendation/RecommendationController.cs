using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DoAn.Core.Application.Interfaces.Recommendation;
using DoAn.Core.Application.DTOs.Base;
using DoAn.Core.Application.DTOs.Recommendation;

namespace DoAn.Presentation.Api.Controllers.Recommendation;

[ApiController]
[Route("api/recommendations")]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    /// <summary>
    /// Gợi ý sân tương tự (Item-to-Item)
    /// Dùng khi user xem chi tiết 1 sân
    /// </summary>
    /// <param name="fieldId">ID của sân hiện tại</param>
    /// <param name="topK">Số lượng sân tương tự (mặc định 10)</param>
    /// <remarks>
    /// Tự động lọc sân cùng tỉnh với sân hiện tại
    /// Ví dụ: GET /api/recommendations/similar/5?topK=5
    /// </remarks>
    [HttpGet("similar/{fieldId}")]
    public async Task<IActionResult> GetSimilarFields(
        [FromRoute] int fieldId,
        [FromQuery] int topK = 10)
    {
        try
        {
            var result = await _recommendationService.GetSimilarFieldsAsync(fieldId, topK);
            
            if (!result.Fields.Any())
            {
                return Ok(ApiResponse<RecommendationResponse>.Fail("Không tìm thấy sân tương tự", 404));
            }

            return Ok(ApiResponse<RecommendationResponse>.Ok(result, "Lấy gợi ý thành công"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Gợi ý sân cho user mới (Location-based + Popularity)
    /// Dùng khi user chưa có lịch sử booking
    /// </summary>
    /// <param name="province">Tỉnh/Thành phố (optional). Nếu không truyền, sẽ lấy tất cả sân</param>
    /// <param name="ward">Phường/Xã (optional). Chỉ áp dụng khi có province</param>
    /// <param name="topK">Số lượng sân gợi ý (mặc định 10)</param>
    /// <remarks>
    /// Ví dụ:
    /// - Tất cả sân: GET /api/recommendations/new-user
    /// - Theo tỉnh: GET /api/recommendations/new-user?province=Hồ Chí Minh
    /// - Theo tỉnh và phường: GET /api/recommendations/new-user?province=Hồ Chí Minh&amp;ward=Phường 1
    /// </remarks>
    [HttpGet("new-user")]
    public async Task<IActionResult> GetRecommendationsForNewUser(
        [FromQuery] string? province,
        [FromQuery] string? ward,
        [FromQuery] int topK = 10)
    {
        try
        {
            var result = await _recommendationService.GetRecommendationsForNewUserAsync(province, ward, topK);
            
            return Ok(ApiResponse<RecommendationResponse>.Ok(result, "Lấy gợi ý thành công"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Gợi ý cá nhân hóa (Content-based)
    /// Dùng khi user đã có lịch sử booking
    /// </summary>
    /// <param name="province">Tỉnh/Thành phố (optional). Nếu không truyền, sẽ gợi ý từ tất cả sân</param>
    /// <param name="topK">Số lượng sân gợi ý (mặc định 10)</param>
    /// <remarks>
    /// Yêu cầu: User phải đăng nhập (JWT token)
    /// Ví dụ:
    /// - Tất cả: GET /api/recommendations/personalized
    /// - Lọc theo tỉnh: GET /api/recommendations/personalized?province=Hồ Chí Minh
    /// </remarks>
    [HttpGet("personalized")]
    [Authorize]
    public async Task<IActionResult> GetPersonalizedRecommendations(
        [FromQuery] string? province,
        [FromQuery] int topK = 10)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(ApiResponse<string>.Fail("Unauthorized", 401));
            }

            var result = await _recommendationService.GetPersonalizedRecommendationsAsync(userId, topK, province);
            
            return Ok(ApiResponse<RecommendationResponse>.Ok(result, "Lấy gợi ý thành công"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Gợi ý tự động (Smart recommendation)
    /// Tự động chọn strategy phù hợp dựa trên user context
    /// </summary>
    /// <param name="province">Tỉnh/Thành phố (optional)</param>
    /// <param name="ward">Phường/Xã (optional)</param>
    /// <param name="topK">Số lượng sân gợi ý (mặc định 10)</param>
    /// <remarks>
    /// Logic:
    /// - Nếu user đã login + có booking → Personalized
    /// - Nếu không → Location-based
    /// 
    /// Ví dụ:
    /// - Tất cả sân: GET /api/recommendations/smart
    /// - Theo vị trí: GET /api/recommendations/smart?province=Hồ Chí Minh
    /// </remarks>
    [HttpGet("smart")]
    public async Task<IActionResult> GetSmartRecommendations(
        [FromQuery] string? province,
        [FromQuery] string? ward,
        [FromQuery] int topK = 10)
    {
        try
        {
            // Nếu user đã login → personalized
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                var personalizedResult = await _recommendationService.GetPersonalizedRecommendationsAsync(userId, topK, province);
                
                // Nếu có kết quả personalized → dùng
                if (personalizedResult.Fields.Any())
                {
                    return Ok(ApiResponse<RecommendationResponse>.Ok(personalizedResult, "Gợi ý cá nhân hóa"));
                }
            }

            // Fallback: location-based cho user mới hoặc không login
            var newUserResult = await _recommendationService.GetRecommendationsForNewUserAsync(province, ward, topK);
            return Ok(ApiResponse<RecommendationResponse>.Ok(newUserResult, "Gợi ý dựa trên vị trí"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }
}
