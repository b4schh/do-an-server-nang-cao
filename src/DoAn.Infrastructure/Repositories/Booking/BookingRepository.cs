using DoAn.Core.Application.Common.Utils;
using DoAn.Core.Application.Interfaces.Booking;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.Booking;

public class BookingRepository : GenericRepository<BookingEntity>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<BookingEntity?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(b => b.Field)
                .ThenInclude(f => f.Complex)
            .Include(b => b.TimeSlot)
            .Include(b => b.Customer)
            .Include(b => b.Owner)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<HashSet<(int FieldId, int TimeSlotId)>> GetBookedTimeSlotIdsForComplexAsync(int complexId, DateTime date)
    {
        var bookedSlots = await _dbSet
            .Where(b => b.Field.ComplexId == complexId
                        && b.BookingDate.Date == date.Date
                        && b.BookingStatus != BookingStatus.Cancelled
                        && b.BookingStatus != BookingStatus.Rejected
                        && b.BookingStatus != BookingStatus.Expired)
            .Select(b => new { b.FieldId, b.TimeSlotId })
            .ToListAsync();

        return bookedSlots.Select(b => (b.FieldId, b.TimeSlotId)).ToHashSet();
    }

    public async Task<Dictionary<string, HashSet<(int FieldId, int TimeSlotId)>>> GetBookedTimeSlotIdsForDateRangeAsync(int complexId, DateTime startDate, DateTime endDate)
    {
        // Lấy tất cả bookings trong khoảng thời gian
        var bookedSlots = await _dbSet
            .Where(b => b.Field.ComplexId == complexId
                        && b.BookingDate.Date >= startDate.Date
                        && b.BookingDate.Date <= endDate.Date
                        && b.BookingStatus != BookingStatus.Cancelled
                        && b.BookingStatus != BookingStatus.Rejected
                        && b.BookingStatus != BookingStatus.Expired)
            .Select(b => new
            {
                Date = b.BookingDate.Date,
                FieldId = b.FieldId,
                TimeSlotId = b.TimeSlotId
            })
            .ToListAsync();

        // Group theo date và tạo dictionary
        var result = new Dictionary<string, HashSet<(int FieldId, int TimeSlotId)>>();

        // Initialize tất cả các ngày trong range
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            result[date.ToString("yyyy-MM-dd")] = new HashSet<(int FieldId, int TimeSlotId)>();
        }

        // Populate bookings vào từng ngày
        foreach (var slot in bookedSlots)
        {
            var dateKey = slot.Date.ToString("yyyy-MM-dd");
            if (result.ContainsKey(dateKey))
            {
                result[dateKey].Add((slot.FieldId, slot.TimeSlotId));
            }
        }

        return result;
    }

    public async Task<IEnumerable<BookingEntity>> GetByCustomerAsync(int customerId, BookingStatus? status = null)
    {
        var query = _dbSet
            .Include(b => b.Field)
                .ThenInclude(f => f.Complex)
            .Include(b => b.TimeSlot)
            .Include(b => b.Owner)
            .Include(b => b.ApprovedByUser)
            .Include(b => b.CancelledByUser)
            .Where(b => b.CustomerId == customerId);

        if (status.HasValue)
        {
            query = query.Where(b => b.BookingStatus == status.Value);
        }

        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<BookingEntity>> GetByOwnerAsync(int ownerId, BookingStatus? status = null)
    {
        var query = _dbSet
            .Include(b => b.Field)
                .ThenInclude(f => f.Complex)
            .Include(b => b.TimeSlot)
            .Include(b => b.Customer)
            .Include(b => b.ApprovedByUser)
            .Include(b => b.CancelledByUser)
            .Where(b => b.OwnerId == ownerId);

        if (status.HasValue)
        {
            query = query.Where(b => b.BookingStatus == status.Value);
        }

        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    public async Task<BookingEntity?> GetDetailAsync(int id)
    {
        return await _dbSet
            .Include(b => b.Field)
                .ThenInclude(f => f.Complex)
            .Include(b => b.TimeSlot)
            .Include(b => b.Customer)
            .Include(b => b.Owner)
            .Include(b => b.ApprovedByUser)
            .Include(b => b.CancelledByUser)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<bool> IsTimeSlotBookedAsync(int fieldId, DateTime bookingDate, int timeSlotId)
    {
        return await _dbSet.AnyAsync(b =>
            b.FieldId == fieldId
            && b.BookingDate.Date == bookingDate.Date
            && b.TimeSlotId == timeSlotId
            && b.BookingStatus != BookingStatus.Cancelled
            && b.BookingStatus != BookingStatus.Rejected
            && b.BookingStatus != BookingStatus.Expired);
    }

    public async Task<IEnumerable<BookingEntity>> GetExpiredPendingBookingsAsync()
    {
        var now = TimeZoneHelper.VietnamNow;
        return await _dbSet
            .Where(b => b.BookingStatus == BookingStatus.Pending
                        && b.HoldExpiresAt < now)
            .ToListAsync();
    }

    public async Task<List<BookingEntity>> GetBookingsForComplexAsync(int complexId, DateOnly startDate, DateOnly endDate)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        var endDateTime = endDate.ToDateTime(TimeOnly.MinValue);

        return await _dbSet
            .Include(b => b.Field)
            .Include(b => b.TimeSlot)
            .Where(b => b.Field.ComplexId == complexId
                        && b.BookingDate.Date >= startDateTime
                        && b.BookingDate.Date <= endDateTime
                        && b.BookingStatus != BookingStatus.Cancelled
                        && b.BookingStatus != BookingStatus.Rejected
                        && b.BookingStatus != BookingStatus.Expired)
            .ToListAsync();
    }

    public async Task<IEnumerable<BookingEntity>> GetBookingsForOwnerAsync(int ownerId)
    {
        return await _dbSet
            .Include(b => b.Field)
                .ThenInclude(f => f.Complex)
            .Include(b => b.TimeSlot)
            .Include(b => b.Customer)
            .Where(b => b.Field.Complex.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<BookingEntity>> GetUserBookingHistoryAsync(int userId)
    {
        return await _dbSet
            .Include(b => b.Field)
                .ThenInclude(f => f.Complex)
            .Include(b => b.TimeSlot)
            .Where(b => b.CustomerId == userId
                     && (b.BookingStatus == BookingStatus.Completed || b.BookingStatus == BookingStatus.Confirmed))
            .OrderByDescending(b => b.BookingDate)
            .Take(100) // Limit to last 100 bookings for performance
            .ToListAsync();
    }

    /// <summary>
    /// Get completed booking counts for multiple complexes (optimized for recommendation)
    /// OPTIMIZED: Single query with GroupBy at database level
    /// </summary>
    public async Task<Dictionary<int, int>> GetComplexBookingCountsAsync(List<int> complexIds)
    {
        if (!complexIds.Any())
            return new Dictionary<int, int>();

        return await _dbSet
            .Where(b => complexIds.Contains(b.Field.ComplexId) 
                     && b.BookingStatus == BookingStatus.Completed)
            .GroupBy(b => b.Field.ComplexId)
            .Select(g => new { ComplexId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ComplexId, x => x.Count);
    }

    /// <summary>
    /// Get booking counts grouped by status (optimized for admin dashboard)
    /// </summary>
    public async Task<Dictionary<string, int>> GetBookingCountsByStatusAsync()
    {
        return await _dbSet
            .GroupBy(b => b.BookingStatus)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    /// <summary>
    /// Get revenue by date range (optimized with database aggregation)
    /// </summary>
    public async Task<Dictionary<DateTime, decimal>> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate
                     && (b.BookingStatus == BookingStatus.Completed 
                      || b.BookingStatus == BookingStatus.Confirmed 
                      || b.BookingStatus == BookingStatus.NoShow))
            .GroupBy(b => b.BookingDate.Date)
            .Select(g => new 
            { 
                Date = g.Key,
                Revenue = g.Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount)
            })
            .ToDictionaryAsync(x => x.Date, x => x.Revenue);
    }

    /// <summary>
    /// Get top complexes by revenue (optimized with single query)
    /// </summary>
    public async Task<List<(int ComplexId, string ComplexName, string OwnerName, int BookingCount, decimal Revenue)>> 
        GetTopComplexesByRevenueAsync(int limit)
    {
        var results = await _dbSet
            .Include(b => b.Field)
                .ThenInclude(f => f.Complex)
            .Include(b => b.Owner)
            .Where(b => b.Field != null && b.Field.Complex != null)
            .GroupBy(b => new
            {
                ComplexId = b.Field.Complex.Id,
                ComplexName = b.Field.Complex.Name,
                OwnerFirstName = b.Owner != null ? b.Owner.FirstName : "",
                OwnerLastName = b.Owner != null ? b.Owner.LastName : ""
            })
            .Select(g => new
            {
                g.Key.ComplexId,
                g.Key.ComplexName,
                OwnerName = (g.Key.OwnerLastName + " " + g.Key.OwnerFirstName).Trim(),
                BookingCount = g.Count(),
                Revenue = g.Where(b => b.BookingStatus == BookingStatus.Completed 
                               || b.BookingStatus == BookingStatus.Confirmed 
                               || b.BookingStatus == BookingStatus.NoShow)
                           .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(limit)
            .ToListAsync();

        return results.Select(r => (r.ComplexId, r.ComplexName, string.IsNullOrEmpty(r.OwnerName) ? "N/A" : r.OwnerName, r.BookingCount, r.Revenue)).ToList();
    }

    /// <summary>
    /// Get top customers by total spending (optimized with single query)
    /// </summary>
    public async Task<List<(int CustomerId, string CustomerName, string Phone, int BookingCount, decimal TotalSpent)>> 
        GetTopCustomersBySpendingAsync(int limit)
    {
        var results = await _dbSet
            .Include(b => b.Customer)
            .Where(b => b.Customer != null)
            .GroupBy(b => new
            {
                CustomerId = b.CustomerId,
                FirstName = b.Customer!.FirstName,
                LastName = b.Customer.LastName,
                Phone = b.Customer.Phone
            })
            .Select(g => new
            {
                g.Key.CustomerId,
                CustomerName = (g.Key.LastName + " " + g.Key.FirstName).Trim(),
                Phone = g.Key.Phone ?? "N/A",
                BookingCount = g.Count(),
                TotalSpent = g.Where(b => b.BookingStatus == BookingStatus.Completed 
                                  || b.BookingStatus == BookingStatus.Confirmed 
                                  || b.BookingStatus == BookingStatus.NoShow)
                              .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(limit)
            .ToListAsync();

        return results.Select(r => (r.CustomerId, r.CustomerName, r.Phone, r.BookingCount, r.TotalSpent)).ToList();
    }
    
    // Admin only - Get all bookings with navigation properties
    public async Task<IEnumerable<BookingEntity>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(b => b.Field)
                .ThenInclude(f => f.Complex)
            .Include(b => b.TimeSlot)
            .Include(b => b.Customer)
            .Include(b => b.Owner)
            .Include(b => b.ApprovedByUser)
            .Include(b => b.CancelledByUser)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    // Admin only - Get queryable with all navigation properties for advanced filtering
    public IQueryable<BookingEntity> GetQueryableWithDetails()
    {
        return _dbSet
            .Include(b => b.Field)
                .ThenInclude(f => f.Complex)
            .Include(b => b.TimeSlot)
            .Include(b => b.Customer)
            .Include(b => b.Owner)
            .Include(b => b.ApprovedByUser)
            .Include(b => b.CancelledByUser);
    }
}
