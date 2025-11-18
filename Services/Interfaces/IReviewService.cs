using FootballField.API.Dtos.Review;

namespace FootballField.API.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<IEnumerable<ReviewDto>> GetReviewsByFieldIdAsync(int fieldId);
        Task<IEnumerable<ReviewDto>> GetReviewsByComplexIdAsync(int complexId);
        Task<IEnumerable<ReviewDto>> GetMyReviewsAsync(int customerId);
        Task<double> GetAverageRatingByFieldIdAsync(int fieldId);
        Task<double> GetAverageRatingByComplexIdAsync(int complexId);
        Task<ReviewDto> CreateReviewAsync(int customerId, CreateReviewDto createReviewDto);
        Task UpdateReviewAsync(int id, int customerId, UpdateReviewDto updateReviewDto);
        Task DeleteReviewAsync(int id, int customerId);
        Task AdminDeleteReviewAsync(int id);
        Task AdminToggleVisibilityAsync(int id, bool isVisible);
    }
}