using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.Booking;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Services.Interfaces;

namespace FootballField.API.Services.Implements
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public BookingService(IBookingRepository bookingRepository, IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BookingDto>>(bookings);
        }

        public async Task<(IEnumerable<BookingDto> bookings, int totalCount)> GetPagedBookingsAsync(int pageIndex, int pageSize)
        {
            var (bookings, totalCount) = await _bookingRepository.GetPagedAsync(pageIndex, pageSize);
            var bookingDtos = _mapper.Map<IEnumerable<BookingDto>>(bookings);
            return (bookingDtos, totalCount);
        }

        public async Task<BookingDto?> GetBookingByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            return booking == null ? null : _mapper.Map<BookingDto>(booking);
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByCustomerIdAsync(int customerId)
        {
            var bookings = await _bookingRepository.GetByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<BookingDto>>(bookings);
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsByOwnerIdAsync(int ownerId)
        {
            var bookings = await _bookingRepository.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<IEnumerable<BookingDto>>(bookings);
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int fieldId, DateTime bookingDate, int timeSlotId)
        {
            return await _bookingRepository.IsTimeSlotAvailableAsync(fieldId, bookingDate, timeSlotId);
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto createBookingDto)
        {
            var booking = _mapper.Map<Booking>(createBookingDto);
            booking.CreatedAt = DateTime.Now;
            booking.UpdatedAt = DateTime.Now;
            
            var created = await _bookingRepository.AddAsync(booking);
            return _mapper.Map<BookingDto>(created);
        }

        public async Task UpdateBookingAsync(int id, UpdateBookingDto updateBookingDto)
        {
            var existingBooking = await _bookingRepository.GetByIdAsync(id);
            if (existingBooking == null)
                throw new Exception("Booking not found");

            _mapper.Map(updateBookingDto, existingBooking);
            existingBooking.UpdatedAt = DateTime.Now;
            
            await _bookingRepository.UpdateAsync(existingBooking);
        }

        public async Task CancelBookingAsync(int id, int cancelledBy, string cancellationReason)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking != null)
            {
                booking.BookingStatus = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.Now;
                booking.CancelledBy = cancelledBy;
                booking.CancellationReason = cancellationReason;
                await _bookingRepository.UpdateAsync(booking);
            }
        }

        public async Task CompleteBookingAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking != null)
            {
                booking.IsCompleted = true;
                await _bookingRepository.UpdateAsync(booking);
            }
        }
    }
}
