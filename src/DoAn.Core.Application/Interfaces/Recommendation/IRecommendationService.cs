using DoAn.Core.Application.DTOs.Recommendation;

namespace DoAn.Core.Application.Interfaces.Recommendation;

public interface IRecommendationService
{
    /// <summary>
    /// Gợi ý sân tương tự dựa trên Item-to-Item similarity
    /// Áp dụng: User bấm vào chi tiết 1 sân
    /// </summary>
    Task<RecommendationResponse> GetSimilarFieldsAsync(int fieldId, int topK = 10);

    /// <summary>
    /// Gợi ý sân cho user mới dựa trên Location + Popularity
    /// Áp dụng: User mới tạo tài khoản, chưa có booking
    /// </summary>
    Task<RecommendationResponse> GetRecommendationsForNewUserAsync(string? province, string? ward, int topK = 10);

    /// <summary>
    /// Gợi ý sân cá nhân hóa dựa trên Content-Based Filtering
    /// Áp dụng: User đã có lịch sử booking
    /// </summary>
    Task<RecommendationResponse> GetPersonalizedRecommendationsAsync(int userId, int topK = 10, string? province = null);
}
