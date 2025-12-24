using FootballField.API.Database;
using FootballField.API.Modules.ReviewManagement.Entities;
using FootballField.API.Modules.ReviewManagement.Dtos;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.ReviewManagement.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetByFieldIdAsync(int fieldId)
        {
            return await _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                        .ThenInclude(f => f.Complex)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Images)
                .Include(r => r.HelpfulVotes)
                .Where(r => r.Booking.FieldId == fieldId && r.IsVisible && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByComplexIdAsync(int complexId)
        {
            return await _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                        .ThenInclude(f => f.Complex)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Images)
                .Include(r => r.HelpfulVotes)
                .Where(r => r.Booking.Field.ComplexId == complexId && r.IsVisible && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                        .ThenInclude(f => f.Complex)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Images)
                .Include(r => r.HelpfulVotes)
                .Where(r => r.Booking.CustomerId == customerId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingByFieldIdAsync(int fieldId)
        {
            var reviews = await _dbSet
                .Include(r => r.Booking)
                .Where(r => r.Booking.FieldId == fieldId && r.IsVisible && !r.IsDeleted)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }

        public async Task<double> GetAverageRatingByComplexIdAsync(int complexId)
        {
            var reviews = await _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                .Where(r => r.Booking.Field.ComplexId == complexId && r.IsVisible && !r.IsDeleted)
                .ToListAsync();

            return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        }

        public async Task<Review?> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                        .ThenInclude(f => f.Complex)
                .Include(r => r.Images)
                .Include(r => r.HelpfulVotes)
                .FirstOrDefaultAsync(r => r.BookingId == bookingId && !r.IsDeleted);
        }

        public async Task<bool> HasReviewForBookingAsync(int bookingId)
        {
            return await _dbSet.AnyAsync(r => r.BookingId == bookingId && !r.IsDeleted);
        }

        public override async Task<Review?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                        .ThenInclude(f => f.Complex)
                .Include(r => r.Images)
                .Include(r => r.HelpfulVotes)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public override async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                        .ThenInclude(f => f.Complex)
                .Include(r => r.Images)
                .Include(r => r.HelpfulVotes)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Review> Reviews, int TotalCount)> GetComplexReviewsWithPaginationAsync(
            int complexId, int pageIndex, int pageSize)
        {
            var query = _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                        .ThenInclude(f => f.Complex)
                .Include(r => r.Images)
                .Include(r => r.HelpfulVotes)
                .Where(r => r.Booking.Field.ComplexId == complexId && r.IsVisible && !r.IsDeleted);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<ReviewStatisticsDto> GetReviewStatisticsAsync(int complexId)
        {
            var reviews = await _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                .Where(r => r.Booking.Field.ComplexId == complexId && r.IsVisible && !r.IsDeleted)
                .ToListAsync();

            var statistics = new ReviewStatisticsDto
            {
                TotalReviews = reviews.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0
            };

            // Count ratings
            foreach (var review in reviews)
            {
                if (statistics.RatingCounts.ContainsKey(review.Rating))
                {
                    statistics.RatingCounts[review.Rating]++;
                }
            }

            return statistics;
        }

        public async Task<int> GetCustomerCompletedBookingsCountAsync(int customerId, int complexId)
        {
            return await _context.Bookings
                .Include(b => b.Field)
                .Where(b => b.CustomerId == customerId 
                    && b.Field.ComplexId == complexId 
                    && b.BookingStatus == BookingManagement.Entities.BookingStatus.Completed)
                .CountAsync();
        }

        public async Task<(IEnumerable<Review> Reviews, int TotalCount)> GetOwnerReviewsWithPaginationAsync(
            int ownerId, int pageIndex, int pageSize, int? complexId, int? rating, bool? isVisible)
        {
            // Start with base query including all necessary navigation properties
            var query = _dbSet
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(r => r.Booking)
                    .ThenInclude(b => b.Field)
                        .ThenInclude(f => f.Complex)
                .Include(r => r.Images)
                .Include(r => r.HelpfulVotes)
                .AsQueryable();

            // Filter by owner - only show reviews for complexes owned by this owner
            query = query.Where(r => !r.IsDeleted && r.Booking.Field.Complex.OwnerId == ownerId);

            // Apply optional filters
            if (complexId.HasValue)
            {
                query = query.Where(r => r.Booking.Field.ComplexId == complexId.Value);
            }

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            if (isVisible.HasValue)
            {
                query = query.Where(r => r.IsVisible == isVisible.Value);
            }

            // Order by created date descending
            query = query.OrderByDescending(r => r.CreatedAt);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var reviews = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }
    }
}