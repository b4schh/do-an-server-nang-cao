using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
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
    }
}