using FootballField.API.Shared.Base;
using FootballField.API.Modules.ReviewManagement.Entities;
using FootballField.API.Modules.ReviewManagement.Dtos;

namespace FootballField.API.Modules.ReviewManagement.Repositories
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetByFieldIdAsync(int fieldId);
        Task<IEnumerable<Review>> GetByComplexIdAsync(int complexId);
        Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId);
        Task<double> GetAverageRatingByFieldIdAsync(int fieldId);
        Task<double> GetAverageRatingByComplexIdAsync(int complexId);
        Task<Review?> GetByBookingIdAsync(int bookingId);
        Task<bool> HasReviewForBookingAsync(int bookingId);
        Task<(IEnumerable<Review> Reviews, int TotalCount)> GetComplexReviewsWithPaginationAsync(
            int complexId, int pageIndex, int pageSize);
        Task<ReviewStatisticsDto> GetReviewStatisticsAsync(int complexId);
        Task<int> GetCustomerCompletedBookingsCountAsync(int customerId, int complexId);
    }
}