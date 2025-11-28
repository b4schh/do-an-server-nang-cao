using FootballField.API.Modules.BookingManagement.Entities;
using FootballField.API.Shared.Dtos.BookingManagement;

namespace FootballField.API.Modules.BookingManagement.Services
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(int customerId, CreateBookingDto dto);
        Task<BookingDto> UploadPaymentProofAsync(int bookingId, int customerId, UploadPaymentProofDto dto);
        Task<BookingDto> ApproveBookingAsync(int bookingId, int ownerId);
        Task<BookingDto> RejectBookingAsync(int bookingId, int ownerId, string? reason);
        Task<BookingDto> CancelBookingAsync(int bookingId, int userId);
        Task<BookingDto> MarkCompletedAsync(int bookingId, int ownerId);
        Task<BookingDto> MarkNoShowAsync(int bookingId, int ownerId);
        Task<IEnumerable<BookingDto>> GetBookingsForCustomerAsync(int customerId, BookingStatus? status = null);
        Task<IEnumerable<BookingDto>> GetBookingsForOwnerAsync(int ownerId, BookingStatus? status = null);
        Task<BookingDto?> GetBookingByIdAsync(int id);
        Task ProcessExpiredBookingsAsync();
        Task AdminForceCompleteBookingAsync(int bookingId);
    }
}
