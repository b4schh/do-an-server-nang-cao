using FootballField.API.DbContexts;
using FootballField.API.Entities;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Utils;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Repositories.Implements
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
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
    }
}
