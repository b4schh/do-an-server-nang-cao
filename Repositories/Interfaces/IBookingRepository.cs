using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Booking>> GetByOwnerIdAsync(int ownerId);
        Task<IEnumerable<Booking>> GetByFieldIdAsync(int fieldId);
        Task<bool> IsTimeSlotAvailableAsync(int fieldId, DateTime bookingDate, int timeSlotId);
        Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int userId);
        Task<List<int>> GetBookedTimeSlotIdsForComplexAsync(int complexId, DateTime date);

    }
}
