using DoAn.Core.Application.DTOs.Review;
using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.Review;

public interface IReviewRepository : IGenericRepository<ReviewEntity>
{
    Task<IEnumerable<ReviewEntity>> GetByFieldIdAsync(int fieldId);
    Task<IEnumerable<ReviewEntity>> GetByComplexIdAsync(int complexId);
    Task<IEnumerable<ReviewEntity>> GetByCustomerIdAsync(int customerId);
    Task<double> GetAverageRatingByFieldIdAsync(int fieldId);
    Task<double> GetAverageRatingByComplexIdAsync(int complexId);
    Task<ReviewEntity?> GetByBookingIdAsync(int bookingId);
    Task<bool> HasReviewForBookingAsync(int bookingId);
    Task<(IEnumerable<ReviewEntity> Reviews, int TotalCount)> GetComplexReviewsWithPaginationAsync(
        int complexId, int pageIndex, int pageSize);
    Task<ReviewStatisticsDto> GetReviewStatisticsAsync(int complexId);
    Task<int> GetCustomerCompletedBookingsCountAsync(int customerId, int complexId);
    Task<(IEnumerable<ReviewEntity> Reviews, int TotalCount)> GetOwnerReviewsWithPaginationAsync(
        int ownerId, int pageIndex, int pageSize, int? complexId, int? rating, bool? isVisible);
}