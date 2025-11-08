using FootballField.API.DbContexts;
using FootballField.API.Entities;
using FootballField.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Repositories.Implements
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetByFieldIdAsync(int fieldId)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Where(r => r.FieldId == fieldId && r.IsVisible && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(r => r.Field)
                .Where(r => r.CustomerId == customerId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingByFieldIdAsync(int fieldId)
        {
            var reviews = await _dbSet
                .Where(r => r.FieldId == fieldId && r.IsVisible && !r.IsDeleted)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }
    }
}
