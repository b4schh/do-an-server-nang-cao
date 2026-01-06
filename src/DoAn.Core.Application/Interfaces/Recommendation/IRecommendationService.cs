using DoAn.Core.Application.DTOs.Recommendation;

namespace DoAn.Core.Application.Interfaces.Recommendation;

public interface IRecommendationService
{
    /// <summary>
    /// Gợi ý Complex tương tự dựa trên Item-to-Item similarity
    /// Áp dụng: User xem chi tiết 1 Complex
    /// </summary>
    Task<RecommendationResponse> GetSimilarComplexesAsync(int complexId, int topK = 10);

    /// <summary>
    /// Gợi ý Complex cho user mới dựa trên Location + Popularity
    /// Áp dụng: User mới tạo tài khoản, chưa có booking
    /// </summary>
    Task<RecommendationResponse> GetRecommendationsForNewUserAsync(string? province, string? ward, int topK = 10);

    /// <summary>
    /// Gợi ý Complex cá nhân hóa dựa trên Content-Based Filtering với 3-Tier Location Priority
    /// Áp dụng: User đã có lịch sử booking
    /// 3-TIER: Same ward (100%) > Same province (85%) > Other province (60%)
    /// </summary>
    Task<RecommendationResponse> GetPersonalizedRecommendationsAsync(int userId, int topK = 10, string? province = null, string? ward = null);
    
    /// <summary>
    /// Smart recommendation - Tự động chọn strategy phù hợp
    /// </summary>
    Task<RecommendationResponse> GetSmartRecommendationsAsync(int? userId, string? province, string? ward, int topK = 10);
}
