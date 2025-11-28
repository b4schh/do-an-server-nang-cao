using FootballField.API.Database;
using FootballField.API.Modules.BookingManagement.Entities;
using FootballField.API.Shared.Base;
using FootballField.API.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.BookingManagement.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Booking?> GetByIdAsync(int id)
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
                .Select(b => new { 
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

        public async Task<IEnumerable<Booking>> GetByCustomerAsync(int customerId, BookingStatus? status = null)
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

        public async Task<IEnumerable<Booking>> GetByOwnerAsync(int ownerId, BookingStatus? status = null)
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

        public async Task<Booking?> GetDetailAsync(int id)
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

        public async Task<IEnumerable<Booking>> GetExpiredPendingBookingsAsync()
        {
            var now = TimeZoneHelper.VietnamNow;
            return await _dbSet
                .Where(b => b.BookingStatus == BookingStatus.Pending 
                            && b.HoldExpiresAt < now)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsForComplexAsync(int complexId, DateOnly startDate, DateOnly endDate)
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
    }
}
