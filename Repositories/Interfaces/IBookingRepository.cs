using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<HashSet<(int FieldId, int TimeSlotId)>> GetBookedTimeSlotIdsForComplexAsync(int complexId, DateTime date);
        Task<IEnumerable<Booking>> GetByCustomerAsync(int customerId, BookingStatus? status = null);
        Task<IEnumerable<Booking>> GetByOwnerAsync(int ownerId, BookingStatus? status = null);
        Task<Booking?> GetDetailAsync(int id);
        Task<bool> IsTimeSlotBookedAsync(int fieldId, DateTime bookingDate, int timeSlotId);
        Task<IEnumerable<Booking>> GetExpiredPendingBookingsAsync();
    }
}
