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
