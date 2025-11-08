using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetByFieldIdAsync(int fieldId);
        Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId);
        Task<double> GetAverageRatingByFieldIdAsync(int fieldId);
        
    }
}
