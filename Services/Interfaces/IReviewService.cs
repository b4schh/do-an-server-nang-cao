using FootballField.API.Dtos.Review;

namespace FootballField.API.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<IEnumerable<ReviewDto>> GetReviewsByFieldIdAsync(int fieldId);
        Task<double> GetAverageRatingByFieldIdAsync(int fieldId);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto createReviewDto);
        Task UpdateReviewAsync(int id, UpdateReviewDto updateReviewDto);
        Task SoftDeleteReviewAsync(int id);
    }
}
