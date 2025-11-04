using FootballField.API.DbContexts;
using FootballField.API.Entities;
using FootballField.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Repositories.Implements
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByOwnerIdAsync(int ownerId)
        {
            return await _dbSet
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Include(b => b.Customer)
                .Where(b => b.OwnerId == ownerId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByFieldIdAsync(int fieldId)
        {
            return await _dbSet
                .Include(b => b.TimeSlot)
                .Include(b => b.Customer)
                .Where(b => b.FieldId == fieldId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int fieldId, DateTime bookingDate, int timeSlotId)
        {
            return !await _dbSet.AnyAsync(b =>
                b.FieldId == fieldId &&
                b.BookingDate.Date == bookingDate.Date &&
                b.TimeSlotId == timeSlotId &&
                b.BookingStatus != BookingStatus.Cancelled);
        }

        public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int userId)
        {
            var today = DateTime.Today;
            return await _dbSet
                .Include(b => b.Field)
                .Include(b => b.TimeSlot)
                .Where(b => (b.CustomerId == userId || b.OwnerId == userId) &&
                           b.BookingDate >= today &&
                           b.BookingStatus != BookingStatus.Cancelled)
                .OrderBy(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<HashSet<(int FieldId, int TimeSlotId)>> GetBookedTimeSlotIdsForComplexAsync(int complexId, DateTime date)
        {
            var bookedSlots = await _dbSet
                .Where(b => b.Field.ComplexId == complexId 
                            && b.BookingDate.Date == date.Date
                            && b.BookingStatus != BookingStatus.Cancelled)
                .Select(b => new { b.FieldId, b.TimeSlotId })
                .ToListAsync();

            return bookedSlots.Select(b => (b.FieldId, b.TimeSlotId)).ToHashSet();
        }
    }
}