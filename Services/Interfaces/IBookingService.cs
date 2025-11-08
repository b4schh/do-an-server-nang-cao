using FootballField.API.Dtos.Booking;

namespace FootballField.API.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
        Task<(IEnumerable<BookingDto> bookings, int totalCount)> GetPagedBookingsAsync(int pageIndex, int pageSize);
        Task<BookingDto?> GetBookingByIdAsync(int id);
        Task<IEnumerable<BookingDto>> GetBookingsByCustomerIdAsync(int customerId);
        Task<IEnumerable<BookingDto>> GetBookingsByOwnerIdAsync(int ownerId);
        Task<bool> IsTimeSlotAvailableAsync(int fieldId, DateTime bookingDate, int timeSlotId);
        Task<BookingDto> CreateBookingAsync(CreateBookingDto createBookingDto);
        Task UpdateBookingAsync(int id, UpdateBookingDto updateBookingDto);
        Task CancelBookingAsync(int id, int cancelledBy, string cancellationReason);
        Task CompleteBookingAsync(int id);
    }
}
