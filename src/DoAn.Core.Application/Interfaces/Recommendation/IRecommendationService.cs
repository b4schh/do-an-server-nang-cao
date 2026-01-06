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
    /// Gợi ý Complex cá nhân hóa dựa trên Content-Based Filtering
    /// Áp dụng: User đã có lịch sử booking
    /// </summary>
    Task<RecommendationResponse> GetPersonalizedRecommendationsAsync(int userId, int topK = 10, string? province = null);
    
    /// <summary>
    /// Smart recommendation - Tự động chọn strategy phù hợp
    /// </summary>
    Task<RecommendationResponse> GetSmartRecommendationsAsync(int? userId, string? province, string? ward, int topK = 10);
}
