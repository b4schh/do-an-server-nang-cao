using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.Booking;

public interface IBookingRepository : IGenericRepository<BookingEntity>
{
    Task<HashSet<(int FieldId, int TimeSlotId)>> GetBookedTimeSlotIdsForComplexAsync(int complexId, DateTime date);
    /// Lấy danh sách booked timeslots cho complex trong khoảng thời gian (startDate -> endDate)
    /// Key: Date (yyyy-MM-dd), Value: HashSet of (FieldId, TimeSlotId)
    Task<Dictionary<string, HashSet<(int FieldId, int TimeSlotId)>>> GetBookedTimeSlotIdsForDateRangeAsync(int complexId, DateTime startDate, DateTime endDate);
    /// Lấy danh sách bookings cho complex trong khoảng thời gian (startDate -> endDate)
    /// Loại trừ các booking có status: Cancelled, Rejected, Expired
    Task<List<BookingEntity>> GetBookingsForComplexAsync(int complexId, DateOnly startDate, DateOnly endDate);
    Task<IEnumerable<BookingEntity>> GetByCustomerAsync(int customerId, BookingStatus? status = null);
    Task<IEnumerable<BookingEntity>> GetByOwnerAsync(int ownerId, BookingStatus? status = null);
    Task<IEnumerable<BookingEntity>> GetBookingsForOwnerAsync(int ownerId);
    Task<BookingEntity?> GetDetailAsync(int id);
    Task<bool> IsTimeSlotBookedAsync(int fieldId, DateTime bookingDate, int timeSlotId);
    Task<IEnumerable<BookingEntity>> GetExpiredPendingBookingsAsync();
}
