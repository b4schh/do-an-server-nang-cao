using AutoMapper;
using FootballField.API.Modules.BookingManagement.Entities;
using FootballField.API.Modules.BookingManagement.Repositories;
using FootballField.API.Modules.FieldManagement.Repositories;
using FootballField.API.Modules.NotificationManagement.Entities;
using FootballField.API.Modules.NotificationManagement.Services;
using FootballField.API.Modules.UserManagement.Repositories;
using FootballField.API.Shared.Dtos.BookingManagement;
using FootballField.API.Shared.Storage;
using FootballField.API.Shared.Utils;

namespace FootballField.API.Modules.BookingManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        private const decimal DEFAULT_DEPOSIT_RATE = 0.3m; // 30% deposit
        private const int HOLD_MINUTES = 5;

        public BookingService(
            IBookingRepository bookingRepository,
            IFieldRepository fieldRepository,
            ITimeSlotRepository timeSlotRepository,
            IUserRepository userRepository,
            IStorageService storageService,
            IMapper mapper,
            INotificationService notificationService)
        {
            _bookingRepository = bookingRepository;
            _fieldRepository = fieldRepository;
            _timeSlotRepository = timeSlotRepository;
            _userRepository = userRepository;
            _storageService = storageService;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<BookingDto> CreateBookingAsync(int customerId, CreateBookingDto dto)
        {
            var vietnamNow = TimeZoneHelper.VietnamNow;

            // Validate booking date
            if (dto.BookingDate.Date < vietnamNow.Date)
                throw new InvalidOperationException("Không thể đặt sân cho ngày trong quá khứ");

            // Get field and timeslot
            var field = await _fieldRepository.GetFieldWithComplexAsync(dto.FieldId)
                        ?? throw new InvalidOperationException("Sân không tồn tại");
            var timeSlot = await _timeSlotRepository.GetByIdAsync(dto.TimeSlotId)
                        ?? throw new InvalidOperationException("Khung giờ không tồn tại");

            if (timeSlot.FieldId != dto.FieldId)
                throw new InvalidOperationException("Khung giờ không thuộc sân này");

            // Check booking in today
            if (dto.BookingDate.Date == vietnamNow.Date && timeSlot.StartTime <= vietnamNow.TimeOfDay)
                throw new InvalidOperationException("Không thể đặt khung giờ đã qua trong ngày hiện tại");

            // Check timeslot availability
            if (await _bookingRepository.IsTimeSlotBookedAsync(dto.FieldId, dto.BookingDate, dto.TimeSlotId))
                throw new InvalidOperationException("Khung giờ này đã được đặt");

            var ownerId = field.Complex.OwnerId;
            var totalAmount = timeSlot.Price;
            var depositAmount = totalAmount * DEFAULT_DEPOSIT_RATE;

            var booking = new Booking
            {
                FieldId = dto.FieldId,
                CustomerId = customerId,
                OwnerId = ownerId,
                TimeSlotId = dto.TimeSlotId,
                BookingDate = dto.BookingDate.Date,
                HoldExpiresAt = vietnamNow.AddMinutes(HOLD_MINUTES),
                TotalAmount = totalAmount,
                DepositAmount = depositAmount,
                Note = dto.Note,
                BookingStatus = BookingStatus.Pending
            };

            var created = await _bookingRepository.AddAsync(booking);
            var bookingWithDetails = await _bookingRepository.GetDetailAsync(created.Id);

            // Notify owner
            try
            {
                var ownerNotification = new Notification
                {
                    UserId = ownerId,
                    SenderId = customerId,
                    Title = $"Booking mới #{booking.Id}",
                    Message = $"Khách hàng đã đặt sân {field.Name} cho ngày {dto.BookingDate:yyyy-MM-dd}",
                    Type = NotificationType.Booking,
                    RelatedTable = "Booking",
                    RelatedId = booking.Id,
                    IsRead = false
                };
                await _notificationService.CreateAndPushAsync(ownerNotification);
            }
            catch { }

            return MapToBookingDtoSync(bookingWithDetails!);
        }

        public async Task<BookingDto> UploadPaymentProofAsync(int bookingId, int customerId, UploadPaymentProofDto dto)
        {
            var booking = await _bookingRepository.GetDetailAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Không tìm thấy booking");
            }

            // Validate customer owns this booking
            if (booking.CustomerId != customerId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật booking này");
            }

            // Check booking status
            if (booking.BookingStatus != BookingStatus.Pending)
            {
                throw new InvalidOperationException("Chỉ có thể upload bill cho booking đang ở trạng thái Pending");
            }

            // Check hold expiry
            var vietnamNow = TimeZoneHelper.VietnamNow;
            if (booking.HoldExpiresAt < vietnamNow)
            {
                booking.BookingStatus = BookingStatus.Expired;
                await _bookingRepository.UpdateAsync(booking);
                throw new InvalidOperationException("Booking đã hết hạn giữ chỗ");
            }

            // Upload image to MinIO
            var fileName = $"booking_{bookingId}_{Guid.NewGuid()}{Path.GetExtension(dto.PaymentProofImage.FileName)}";

            using var stream = dto.PaymentProofImage.OpenReadStream();
            var imageUrl = await _storageService.UploadAsync(stream, fileName, dto.PaymentProofImage.ContentType);

            // Update booking
            booking.PaymentProofUrl = imageUrl;
            if (!string.IsNullOrWhiteSpace(dto.PaymentNote))
            {
                booking.Note = dto.PaymentNote;
            }
            booking.BookingStatus = BookingStatus.WaitingForApproval;

            await _bookingRepository.UpdateAsync(booking);

            // Notify owner that a customer uploaded payment proof and is waiting for approval
            try
            {
                var ownerNotification = new Notification
                {
                    UserId = booking.OwnerId,
                    SenderId = booking.CustomerId,
                    Title = $"Đơn #{booking.Id} có bill mới",
                    Message = $"Khách hàng đã upload bill cho đơn #{booking.Id}. Vui lòng kiểm tra và duyệt hoặc từ chối.",
                    Type = NotificationType.Booking,
                    RelatedTable = "Booking",
                    RelatedId = booking.Id,
                    IsRead = false
                };

                await _notificationService.CreateAndPushAsync(ownerNotification);
            }
            catch
            {
                // best-effort: nếu push lỗi thì không block luồng chính
            }

            return await MapToBookingDto(booking);
        }

        public async Task<BookingDto> ApproveBookingAsync(int bookingId, int ownerId)
        {
            var booking = await _bookingRepository.GetDetailAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Không tìm thấy booking");
            }

            // Validate owner
            if (booking.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền duyệt booking này");
            }

            // Check status
            if (booking.BookingStatus != BookingStatus.WaitingForApproval)
            {
                throw new InvalidOperationException("Chỉ có thể duyệt booking đang ở trạng thái WaitingForApproval");
            }

            // Update booking
            booking.BookingStatus = BookingStatus.Confirmed;
            booking.ApprovedBy = ownerId;
            booking.ApprovedAt = TimeZoneHelper.VietnamNow;

            await _bookingRepository.UpdateAsync(booking);

            // Notify customer that booking was approved
            try
            {
                var customerNotification = new Notification
                {
                    UserId = booking.CustomerId,
                    SenderId = booking.OwnerId,
                    Title = $"Đơn #{booking.Id} đã được duyệt",
                    Message = $"Chủ sân đã duyệt đơn #{booking.Id} cho ngày {booking.BookingDate:yyyy-MM-dd}.",
                    Type = NotificationType.Booking,
                    RelatedTable = "Booking",
                    RelatedId = booking.Id,
                    IsRead = false
                };

                await _notificationService.CreateAndPushAsync(customerNotification);
            }
            catch
            {
                // best-effort
            }

            return await MapToBookingDto(booking);
        }

        public async Task<BookingDto> RejectBookingAsync(int bookingId, int ownerId, string? reason)
        {
            var booking = await _bookingRepository.GetDetailAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Không tìm thấy booking");
            }

            // Validate owner
            if (booking.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền từ chối booking này");
            }

            // Check status
            if (booking.BookingStatus != BookingStatus.WaitingForApproval)
            {
                throw new InvalidOperationException("Chỉ có thể từ chối booking đang ở trạng thái WaitingForApproval");
            }

            // Update booking
            booking.BookingStatus = BookingStatus.Rejected;
            if (!string.IsNullOrWhiteSpace(reason))
            {
                booking.Note = $"Từ chối: {reason}";
            }

            await _bookingRepository.UpdateAsync(booking);

            // Notify customer that booking was rejected
            try
            {
                var customerNotification = new Notification
                {
                    UserId = booking.CustomerId,
                    SenderId = booking.OwnerId,
                    Title = $"Đơn #{booking.Id} đã bị từ chối",
                    Message = $"Chủ sân đã từ chối đơn #{booking.Id}. Lý do: {reason ?? "Không có lý do"}",
                    Type = NotificationType.Booking,
                    RelatedTable = "Booking",
                    RelatedId = booking.Id,
                    IsRead = false
                };

                await _notificationService.CreateAndPushAsync(customerNotification);
            }
            catch
            {
                // best-effort
            }

            return await MapToBookingDto(booking);
        }

        public async Task<BookingDto> CancelBookingAsync(int bookingId, int userId)
        {
            var booking = await _bookingRepository.GetDetailAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Không tìm thấy booking");
            }

            // Validate user is customer or owner
            if (booking.CustomerId != userId && booking.OwnerId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền hủy booking này");
            }

            // Check status - can only cancel Pending, WaitingForApproval, or Confirmed
            if (booking.BookingStatus != BookingStatus.Pending
                && booking.BookingStatus != BookingStatus.WaitingForApproval
                && booking.BookingStatus != BookingStatus.Confirmed)
            {
                throw new InvalidOperationException("Không thể hủy booking ở trạng thái này");
            }

            // Update booking
            booking.BookingStatus = BookingStatus.Cancelled;
            booking.CancelledBy = userId;
            booking.CancelledAt = TimeZoneHelper.VietnamNow;

            await _bookingRepository.UpdateAsync(booking);

            // Notify the other party (if customer cancelled notify owner, if owner cancelled notify customer)
            try
            {
                if (userId == booking.CustomerId)
                {
                    var ownerNotification = new Notification
                    {
                        UserId = booking.OwnerId,
                        SenderId = booking.CustomerId,
                        Title = $"Đơn #{booking.Id} đã bị khách hủy",
                        Message = $"Khách hàng đã hủy đơn #{booking.Id}.",
                        Type = NotificationType.Booking,
                        RelatedTable = "Booking",
                        RelatedId = booking.Id,
                        IsRead = false
                    };
                    await _notificationService.CreateAndPushAsync(ownerNotification);
                }
                else
                {
                    var customerNotification = new Notification
                    {
                        UserId = booking.CustomerId,
                        SenderId = booking.OwnerId,
                        Title = $"Đơn #{booking.Id} đã bị chủ sân hủy",
                        Message = $"Chủ sân đã hủy đơn #{booking.Id}.",
                        Type = NotificationType.Booking,
                        RelatedTable = "Booking",
                        RelatedId = booking.Id,
                        IsRead = false
                    };
                    await _notificationService.CreateAndPushAsync(customerNotification);
                }
            }
            catch
            {
                // best-effort
            }

            return await MapToBookingDto(booking);
        }

        public async Task<BookingDto> MarkCompletedAsync(int bookingId, int ownerId)
        {
            var booking = await _bookingRepository.GetDetailAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Không tìm thấy booking");
            }

            // Validate owner
            if (booking.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền đánh dấu booking này");
            }

            // Check status
            if (booking.BookingStatus != BookingStatus.Confirmed)
            {
                throw new InvalidOperationException("Chỉ có thể đánh dấu hoàn thành cho booking đã Confirmed");
            }

            // Check booking date has passed
            var vietnamNow = TimeZoneHelper.VietnamNow;
            if (booking.BookingDate.Date > vietnamNow.Date)
            {
                throw new InvalidOperationException("Chỉ có thể đánh dấu hoàn thành sau ngày đá");
            }

            // Update booking
            booking.BookingStatus = BookingStatus.Completed;

            await _bookingRepository.UpdateAsync(booking);

            // Optionally notify customer about completion
            try
            {
                var customerNotification = new Notification
                {
                    UserId = booking.CustomerId,
                    SenderId = booking.OwnerId,
                    Title = $"Đơn #{booking.Id} đã hoàn thành",
                    Message = $"Đơn #{booking.Id} đã được đánh dấu hoàn thành.",
                    Type = NotificationType.Booking,
                    RelatedTable = "Booking",
                    RelatedId = booking.Id,
                    IsRead = false
                };
                await _notificationService.CreateAndPushAsync(customerNotification);
            }
            catch
            {
                // best-effort
            }

            return await MapToBookingDto(booking);
        }

        public async Task<BookingDto> MarkNoShowAsync(int bookingId, int ownerId)
        {
            var booking = await _bookingRepository.GetDetailAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Không tìm thấy booking");
            }

            // Validate owner
            if (booking.OwnerId != ownerId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền đánh dấu booking này");
            }

            // Check status
            if (booking.BookingStatus != BookingStatus.Confirmed)
            {
                throw new InvalidOperationException("Chỉ có thể đánh dấu NoShow cho booking đã Confirmed");
            }

            // Check booking date has passed
            var vietnamNow = TimeZoneHelper.VietnamNow;
            if (booking.BookingDate.Date > vietnamNow.Date)
            {
                throw new InvalidOperationException("Chỉ có thể đánh dấu NoShow sau ngày đá");
            }

            // Update booking
            booking.BookingStatus = BookingStatus.NoShow;

            await _bookingRepository.UpdateAsync(booking);

            // Optionally notify customer about no-show
            try
            {
                var customerNotification = new Notification
                {
                    UserId = booking.CustomerId,
                    SenderId = booking.OwnerId,
                    Title = $"Đơn #{booking.Id}: Khách không đến",
                    Message = $"Đơn #{booking.Id} được đánh dấu NoShow.",
                    Type = NotificationType.Booking,
                    RelatedTable = "Booking",
                    RelatedId = booking.Id,
                    IsRead = false
                };
                await _notificationService.CreateAndPushAsync(customerNotification);
            }
            catch
            {
                // best-effort
            }

            return await MapToBookingDto(booking);
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsForCustomerAsync(int customerId, BookingStatus? status = null)
        {
            var bookings = await _bookingRepository.GetByCustomerAsync(customerId, status);
            return bookings.Select(MapToBookingDtoSync);
        }

        public async Task<IEnumerable<BookingDto>> GetBookingsForOwnerAsync(int ownerId, BookingStatus? status = null)
        {
            var bookings = await _bookingRepository.GetByOwnerAsync(ownerId, status);
            return bookings.Select(MapToBookingDtoSync);
        }

        public async Task<BookingDto?> GetBookingByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetDetailAsync(id);
            if (booking == null) return null;
            return await MapToBookingDto(booking);
        }

        public async Task ProcessExpiredBookingsAsync()
        {
            var expiredBookings = await _bookingRepository.GetExpiredPendingBookingsAsync();
            foreach (var booking in expiredBookings)
            {
                booking.BookingStatus = BookingStatus.Expired;
            }

            if (expiredBookings.Any())
            {
                await _bookingRepository.UpdateRangeAsync(expiredBookings);
            }
        }

        private async Task<BookingDto> MapToBookingDto(Booking booking)
        {
            // Reload with includes if needed
            if (booking.Field == null || booking.TimeSlot == null)
            {
                booking = await _bookingRepository.GetDetailAsync(booking.Id) ?? booking;
            }

            return MapToBookingDtoSync(booking);
        }

        private BookingDto MapToBookingDtoSync(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                FieldId = booking.FieldId,
                FieldName = booking.Field?.Name,
                ComplexId = booking.Field?.ComplexId ?? 0,
                ComplexName = booking.Field?.Complex?.Name,
                CustomerId = booking.CustomerId,
                CustomerName = $"{booking.Customer?.LastName} {booking.Customer?.FirstName}",
                CustomerPhone = booking.Customer?.Phone,
                OwnerId = booking.OwnerId,
                OwnerName = $"{booking.Owner?.LastName} {booking.Owner?.FirstName}",
                TimeSlotId = booking.TimeSlotId,
                StartTime = booking.TimeSlot?.StartTime,
                EndTime = booking.TimeSlot?.EndTime,
                BookingDate = booking.BookingDate,
                HoldExpiresAt = booking.HoldExpiresAt,
                TotalAmount = booking.TotalAmount,
                DepositAmount = booking.DepositAmount,
                PaymentProofUrl = booking.PaymentProofUrl,
                Note = booking.Note,
                BookingStatus = booking.BookingStatus,
                BookingStatusText = GetBookingStatusText(booking.BookingStatus),
                ApprovedBy = booking.ApprovedBy,
                ApprovedByName = booking.ApprovedByUser != null
                    ? $"{booking.ApprovedByUser.LastName} {booking.ApprovedByUser.FirstName}"
                    : null,
                ApprovedAt = booking.ApprovedAt,
                CancelledBy = booking.CancelledBy,
                CancelledByName = booking.CancelledByUser != null
                    ? $"{booking.CancelledByUser.LastName} {booking.CancelledByUser.FirstName}"
                    : null,
                CancelledAt = booking.CancelledAt,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt
            };
        }

        private string GetBookingStatusText(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.Pending => "Chờ thanh toán cọc",
                BookingStatus.WaitingForApproval => "Chờ duyệt",
                BookingStatus.Confirmed => "Đã xác nhận",
                BookingStatus.Rejected => "Đã từ chối",
                BookingStatus.Cancelled => "Đã hủy",
                BookingStatus.Completed => "Hoàn thành",
                BookingStatus.Expired => "Hết hạn",
                BookingStatus.NoShow => "Không đến",
                _ => "Không xác định"
            };
        }

        public async Task AdminForceCompleteBookingAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new Exception("Không tìm thấy booking");

            // Force update to Completed for testing purposes
            booking.BookingStatus = BookingStatus.Completed;
            booking.UpdatedAt = DateTime.UtcNow.AddHours(7);

            await _bookingRepository.UpdateAsync(booking);
        }
    }
}
