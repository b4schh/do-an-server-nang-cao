using DoAn.Core.Application.Interfaces.Review;
using DoAn.Core.Application.DTOs.Review;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.Review;

public class ReviewRepository : GenericRepository<ReviewEntity>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ReviewEntity>> GetByFieldIdAsync(int fieldId)
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

    public async Task<IEnumerable<ReviewEntity>> GetByComplexIdAsync(int complexId)
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

    public async Task<IEnumerable<ReviewEntity>> GetByCustomerIdAsync(int customerId)
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
        // OPTIMIZED: Calculate average at database level
        var average = await _dbSet
            .Include(r => r.Booking)
            .Where(r => r.Booking.FieldId == fieldId && r.IsVisible && !r.IsDeleted)
            .Select(r => (double?)r.Rating)
            .AverageAsync();

        return average ?? 0;
    }

    public async Task<double> GetAverageRatingByComplexIdAsync(int complexId)
    {
        // OPTIMIZED: Calculate average at database level
        var average = await _dbSet
            .Include(r => r.Booking)
                .ThenInclude(b => b.Field)
            .Where(r => r.Booking.Field.ComplexId == complexId && r.IsVisible && !r.IsDeleted)
            .Select(r => (double?)r.Rating)
            .AverageAsync();

        return average ?? 0;
    }

    public async Task<ReviewEntity?> GetByBookingIdAsync(int bookingId)
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

    public override async Task<ReviewEntity?> GetByIdAsync(int id)
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

    public override async Task<IEnumerable<ReviewEntity>> GetAllAsync()
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

    public async Task<(IEnumerable<ReviewEntity> Reviews, int TotalCount)> GetComplexReviewsWithPaginationAsync(
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
        // OPTIMIZED: Aggregate at database level with GroupBy
        var query = _dbSet
            .Include(r => r.Booking)
                .ThenInclude(b => b.Field)
            .Where(r => r.Booking.Field.ComplexId == complexId && r.IsVisible && !r.IsDeleted);

        var totalCount = await query.CountAsync();
        var averageRating = await query.Select(r => (double?)r.Rating).AverageAsync() ?? 0;
        
        // Get rating distribution with single GroupBy query
        var ratingCounts = await query
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => (int)x.Rating, x => x.Count);

        var statistics = new ReviewStatisticsDto
        {
            TotalReviews = totalCount,
            AverageRating = averageRating
        };

        // Initialize all ratings (1-5) with counts from DB or 0
        for (int i = 1; i <= 5; i++)
        {
            statistics.RatingCounts[i] = ratingCounts.ContainsKey(i) ? ratingCounts[i] : 0;
        }

        return statistics;
    }

    public async Task<int> GetCustomerCompletedBookingsCountAsync(int customerId, int complexId)
    {
        return await _context.Bookings
            .Include(b => b.Field)
            .Where(b => b.CustomerId == customerId
                && b.Field.ComplexId == complexId
                && b.BookingStatus == BookingStatus.Completed)
            .CountAsync();
    }

    public async Task<(IEnumerable<ReviewEntity> Reviews, int TotalCount)> GetOwnerReviewsWithPaginationAsync(
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
