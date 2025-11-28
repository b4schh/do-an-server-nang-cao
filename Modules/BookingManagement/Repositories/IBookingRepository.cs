using FootballField.API.Modules.BookingManagement.Entities;
using FootballField.API.Shared.Base;

namespace FootballField.API.Modules.BookingManagement.Repositories
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<HashSet<(int FieldId, int TimeSlotId)>> GetBookedTimeSlotIdsForComplexAsync(int complexId, DateTime date);
        /// <summary>
        /// Lấy danh sách booked timeslots cho complex trong khoảng thời gian (startDate -> endDate)
        /// Key: Date (yyyy-MM-dd), Value: HashSet of (FieldId, TimeSlotId)
        /// </summary>
        Task<Dictionary<string, HashSet<(int FieldId, int TimeSlotId)>>> GetBookedTimeSlotIdsForDateRangeAsync(int complexId, DateTime startDate, DateTime endDate);
        /// <summary>
        /// Lấy danh sách bookings cho complex trong khoảng thời gian (startDate -> endDate)
        /// Loại trừ các booking có status: Cancelled, Rejected, Expired
        /// </summary>
        Task<List<Booking>> GetBookingsForComplexAsync(int complexId, DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<Booking>> GetByCustomerAsync(int customerId, BookingStatus? status = null);
        Task<IEnumerable<Booking>> GetByOwnerAsync(int ownerId, BookingStatus? status = null);
        Task<Booking?> GetDetailAsync(int id);
        Task<bool> IsTimeSlotBookedAsync(int fieldId, DateTime bookingDate, int timeSlotId);
        Task<IEnumerable<Booking>> GetExpiredPendingBookingsAsync();
    }
}
