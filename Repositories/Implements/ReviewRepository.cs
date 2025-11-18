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
                .Include(r => r.Field)
                .Include(r => r.Complex)
                .Where(r => r.FieldId == fieldId && r.IsVisible && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByComplexIdAsync(int complexId)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Include(r => r.Field)
                .Include(r => r.Complex)
                .Where(r => r.ComplexId == complexId && r.IsVisible && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(r => r.Field)
                .Include(r => r.Complex)
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

        public async Task<double> GetAverageRatingByComplexIdAsync(int complexId)
        {
            var reviews = await _dbSet
                .Where(r => r.ComplexId == complexId && r.IsVisible && !r.IsDeleted)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }

        public async Task<Review?> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Include(r => r.Field)
                .Include(r => r.Complex)
                .FirstOrDefaultAsync(r => r.BookingId == bookingId && !r.IsDeleted);
        }

        public async Task<bool> HasReviewForBookingAsync(int bookingId)
        {
            return await _dbSet.AnyAsync(r => r.BookingId == bookingId && !r.IsDeleted);
        }

        public override async Task<Review?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Include(r => r.Field)
                .Include(r => r.Complex)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public override async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Include(r => r.Field)
                .Include(r => r.Complex)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}