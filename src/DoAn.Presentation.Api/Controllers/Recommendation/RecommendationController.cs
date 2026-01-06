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
    /// Gợi ý cụm sân tương tự (Complex-to-Complex Similarity)
    /// Dùng khi user xem chi tiết 1 cụm sân
    /// </summary>
    /// <param name="complexId">ID của cụm sân hiện tại</param>
    /// <param name="topK">Số lượng cụm sân tương tự (mặc định 10)</param>
    /// <remarks>
    /// Tự động lọc cụm sân cùng tỉnh
    /// Ví dụ: GET /api/recommendations/similar-complex/5?topK=5
    /// </remarks>
    [HttpGet("similar-complex/{complexId}")]
    public async Task<IActionResult> GetSimilarComplexes(
        [FromRoute] int complexId,
        [FromQuery] int topK = 10)
    {
        try
        {
            var result = await _recommendationService.GetSimilarComplexesAsync(complexId, topK);
            
            if (!result.Complexes.Any())
            {
                return Ok(ApiResponse<RecommendationResponse>.Fail("Không tìm thấy cụm sân tương tự", 404));
            }

            return Ok(ApiResponse<RecommendationResponse>.Ok(result, "Lấy gợi ý thành công"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }

    /// <summary>
    /// Gợi ý cụm sân cho user mới (Location-based + Popularity)
    /// Dùng khi user chưa có lịch sử booking
    /// </summary>
    /// <param name="province">Tỉnh/Thành phố (optional)</param>
    /// <param name="ward">Phường/Xã (optional)</param>
    /// <param name="topK">Số lượng cụm sân gợi ý (mặc định 10)</param>
    /// <remarks>
    /// Ví dụ:
    /// - Tất cả: GET /api/recommendations/new-user
    /// - Theo tỉnh: GET /api/recommendations/new-user?province=Hồ Chí Minh
    /// - Chi tiết: GET /api/recommendations/new-user?province=Hồ Chí Minh&amp;ward=Phường 1
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
    /// Gợi ý cụm sân cá nhân hóa (Content-based Filtering)
    /// Dùng khi user đã có lịch sử booking
    /// </summary>
    /// <param name="province">Tỉnh/Thành phố (optional)</param>
    /// <param name="topK">Số lượng cụm sân gợi ý (mặc định 10)</param>
    /// <remarks>
    /// Yêu cầu: User phải đăng nhập (JWT token)
    /// Ví dụ:
    /// - Tất cả: GET /api/recommendations/personalized
    /// - Lọc tỉnh: GET /api/recommendations/personalized?province=Hồ Chí Minh
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
    /// Smart Recommendation - Tự động chọn strategy tốt nhất
    /// </summary>
    /// <param name="province">Tỉnh/Thành phố (optional)</param>
    /// <param name="ward">Phường/Xã (optional)</param>
    /// <param name="topK">Số lượng cụm sân gợi ý (mặc định 10)</param>
    /// <remarks>
    /// Logic:
    /// - Nếu user login + có booking → Personalized
    /// - Nếu không → Location-based
    /// 
    /// Ví dụ:
    /// - GET /api/recommendations/smart
    /// - GET /api/recommendations/smart?province=Hồ Chí Minh
    /// </remarks>
    [HttpGet("smart")]
    public async Task<IActionResult> GetSmartRecommendations(
        [FromQuery] string? province,
        [FromQuery] string? ward,
        [FromQuery] int topK = 10)
    {
        try
        {
            // Lấy userId nếu user đã login
            int? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedUserId))
            {
                userId = parsedUserId;
            }

            var result = await _recommendationService.GetSmartRecommendationsAsync(userId, province, ward, topK);
            
            return Ok(ApiResponse<RecommendationResponse>.Ok(result, "Lấy gợi ý thành công"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Lỗi hệ thống: {ex.Message}", 500));
        }
    }
}
