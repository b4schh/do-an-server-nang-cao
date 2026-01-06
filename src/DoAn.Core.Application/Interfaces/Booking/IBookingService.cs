using DoAn.Core.Application.DTOs.Booking;
using DoAn.Core.Domain.Entities;

namespace DoAn.Core.Application.Interfaces.Booking;

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
    Task<(IEnumerable<BookingDto> bookings, int totalRecords)> GetBookingsForOwnerAsync(int ownerId, int pageIndex, int pageSize, BookingStatus? status = null);
    Task<BookingDto?> GetBookingByIdAsync(int id);
    Task ProcessExpiredBookingsAsync();
    Task AdminForceCompleteBookingAsync(int bookingId);
}
