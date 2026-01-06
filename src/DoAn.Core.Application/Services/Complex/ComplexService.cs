using AutoMapper;
using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Core.Application.Interfaces.Field;
using DoAn.Core.Application.Interfaces.Booking;
using DoAn.Core.Application.Interfaces.User;
using DoAn.Core.Application.Interfaces.SystemConfig;
using DoAn.Core.Application.Interfaces.Notification;
using DoAn.Core.Application.Interfaces.Review;
using DoAn.Core.Application.DTOs.Complex;
using DoAn.Core.Application.DTOs.Field;
using Microsoft.Extensions.Logging;
using DoAn.Core.Application.Common.Utils;

namespace DoAn.Core.Application.Services.Complex
{
    public class ComplexService : IComplexService
    {
        private readonly IComplexRepository _complexRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ISystemConfigService _systemConfigService;
        private readonly INotificationService _notificationService;
        private readonly IReviewRepository _reviewRepository;
        private readonly IComplexImageRepository _complexImageRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ComplexService> _logger;

        public ComplexService(
            IComplexRepository complexRepository,
            IFieldRepository fieldRepository,
            ITimeSlotRepository timeSlotRepository,
            IUserRepository userRepository,
            IBookingRepository bookingRepository,
            ISystemConfigService systemConfigService,
            INotificationService notificationService,
            IReviewRepository reviewRepository,
            IComplexImageRepository complexImageRepository,
            IMapper mapper,
            ILogger<ComplexService> logger)
        {
            _complexRepository = complexRepository;
            _fieldRepository = fieldRepository;
            _timeSlotRepository = timeSlotRepository;
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
            _systemConfigService = systemConfigService;
            _notificationService = notificationService;
            _reviewRepository = reviewRepository;
            _complexImageRepository = complexImageRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ComplexDto>> GetAllComplexesAsync()
        {
            var complexes = await _complexRepository.GetAllAsync(c => !c.IsDeleted);
            return _mapper.Map<IEnumerable<ComplexDto>>(complexes);
        }

        public async Task<(IEnumerable<ComplexDto> complexes, int totalCount)> GetPagedComplexesAsync(int pageIndex, int pageSize)
        {
            var (complexes, totalCount) = await _complexRepository.GetPagedAsync(pageIndex, pageSize, c => !c.IsDeleted);
            var complexDtos = _mapper.Map<IEnumerable<ComplexDto>>(complexes);
            return (complexDtos, totalCount);
        }

        public async Task<ComplexDto?> GetComplexByIdAsync(int id)
        {
            var complex = await _complexRepository.GetByIdAsync(id);
            return complex == null ? null : _mapper.Map<ComplexDto>(complex);
        }

        public async Task<ComplexWithFieldsDto?> GetComplexWithFieldsAsync(int id)
        {
            var complex = await _complexRepository.GetComplexWithFieldsAsync(id);
            return complex == null ? null : _mapper.Map<ComplexWithFieldsDto>(complex);
        }

        public async Task<ComplexFullDetailsDto?> GetComplexWithFullDetailsAsync(int id, DateTime date)
        {
            var complex = await _complexRepository.GetComplexWithFullDetailsAsync(id);
            if (complex == null) return null;

            var complexDto = _mapper.Map<ComplexFullDetailsDto>(complex);

            // Lấy danh sách booking cho ngày được chọn từ Repository
            var bookedTimeSlots = await _bookingRepository.GetBookedTimeSlotIdsForComplexAsync(id, date);

            // Map fields với timeslots và trạng thái availability
            complexDto.Fields = complex.Fields.Select(f => new FieldWithTimeSlotsDto
            {
                Id = f.Id,
                ComplexId = f.ComplexId,
                Name = f.Name,
                SurfaceType = f.SurfaceType,
                FieldSize = f.FieldSize,
                IsActive = f.IsActive,
                TimeSlots = f.TimeSlots.Select(ts => new TimeSlotWithAvailabilityDto
                {
                    Id = ts.Id,
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,
                    Price = ts.Price,
                    IsActive = ts.IsActive,
                    IsBooked = bookedTimeSlots.Contains((f.Id, ts.Id))
                }).OrderBy(ts => ts.StartTime)
            });

            // Map images với IsMain
            complexDto.Images = complex.ComplexImages.Select(img => new ComplexImageResponseDto
            {
                Id = img.Id,
                ComplexId = img.ComplexId,
                ImageUrl = img.ImageUrl,
                IsMain = img.IsMain
            }).OrderByDescending(img => img.IsMain).ThenBy(img => img.Id);

            return complexDto;
        }

        public async Task<ComplexWeeklyDetailsDto?> GetComplexWeeklyDetailsAsync(int id, DateTime startDate, DateTime endDate)
        {
            var complex = await _complexRepository.GetComplexWithFullDetailsAsync(id);
            if (complex == null) return null;

            // Map basic complex info
            var complexDto = _mapper.Map<ComplexWeeklyDetailsDto>(complex);

            // Lấy danh sách booked timeslots cho range ngày
            var bookedTimeSlotsByDate = await _bookingRepository.GetBookedTimeSlotIdsForDateRangeAsync(id, startDate, endDate);

            // Map fields với daily timeslots
            complexDto.Fields = complex.Fields.Select(f =>
            {
                var fieldDto = new FieldWeeklyAvailabilityDto
                {
                    Id = f.Id,
                    ComplexId = f.ComplexId,
                    Name = f.Name,
                    SurfaceType = f.SurfaceType,
                    FieldSize = f.FieldSize,
                    IsActive = f.IsActive,
                    DailyTimeSlots = new Dictionary<string, IEnumerable<DailyTimeSlotDto>>()
                };

                // Tạo timeslots cho từng ngày trong range
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var dateKey = date.ToString("yyyy-MM-dd");
                    var bookedSlotsForDate = bookedTimeSlotsByDate.ContainsKey(dateKey)
                        ? bookedTimeSlotsByDate[dateKey]
                        : new HashSet<(int FieldId, int TimeSlotId)>();

                    fieldDto.DailyTimeSlots[dateKey] = f.TimeSlots.Select(ts => new DailyTimeSlotDto
                    {
                        Id = ts.Id,
                        StartTime = ts.StartTime,
                        EndTime = ts.EndTime,
                        Price = ts.Price,
                        IsActive = ts.IsActive,
                        // IsAvailable = true nếu KHÔNG có trong booked list
                        IsAvailable = !bookedSlotsForDate.Contains((f.Id, ts.Id))
                    }).OrderBy(ts => ts.StartTime).ToList();
                }

                return fieldDto;
            }).ToList();

            return complexDto;
        }

        public async Task<IEnumerable<ComplexDto>> GetComplexesByOwnerIdAsync(int ownerId)
        {
            var complexes = await _complexRepository.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<IEnumerable<ComplexDto>>(complexes);
        }

        public async Task<(IEnumerable<ComplexDto> complexes, int totalCount)> GetComplexesByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize)
        {
            var (complexes, totalCount) = await _complexRepository.GetByOwnerIdPagedAsync(ownerId, pageIndex, pageSize);
            var complexDtos = _mapper.Map<IEnumerable<ComplexDto>>(complexes);
            return (complexDtos, totalCount);
        }

        public async Task<bool> ValidateOwnerRoleAsync(int ownerId)
        {
            var owner = await _userRepository.GetUserByIdWithRoleAsync(ownerId);
            if (owner == null) return false;

            return owner.UserRoles.Any(ur => ur.Role.Name == "Owner" || ur.Role.Name == "Admin");
        }

        public async Task<IEnumerable<ComplexDto>> SearchComplexesAsync(
            string? name,
            string? ward,
            string? province,
            string? surfaceType = null,
            string? fieldSize = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            double? minRating = null,
            double? maxRating = null)
        {
            // Check MAINTENANCE_MODE từ SYSTEM_CONFIG
            var maintenanceMode = await _systemConfigService.GetConfigValueAsync<bool?>("MAINTENANCE_MODE") ?? false;
            if (maintenanceMode)
            {
                // Nếu đang bảo trì, trả về danh sách rỗng
                return Enumerable.Empty<ComplexDto>();
            }

            // Lấy tất cả complexes kèm Fields, TimeSlots, Reviews và OwnerSettings
            var complexesWithBankInfo = await _complexRepository.GetComplexesWithDetailsForSearchAsync();

            // Lọc theo điều kiện hiển thị sân:
            // - SYSTEM_CONFIG.MAINTENANCE_MODE = false (đã check ở trên)
            // - COMPLEX.status = Approved
            // - COMPLEX.is_active = true
            // - COMPLEX.is_deleted = false (đã lọc trong repository)
            // - OWNER_SETTING.bank_account_number IS NOT NULL
            var complexes = complexesWithBankInfo
                .Where(x => x.Complex.Status == ComplexStatus.Approved
                         && x.Complex.IsActive
                         && x.HasBankInfo)
                .Select(x => x.Complex);

            // Filter theo tên
            if (!string.IsNullOrWhiteSpace(name))
            {
                complexes = complexes.Where(c => !string.IsNullOrEmpty(c.Name) &&
                    c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            // Filter theo Ward
            if (!string.IsNullOrWhiteSpace(ward))
            {
                complexes = complexes.Where(c => !string.IsNullOrEmpty(c.Ward) &&
                    c.Ward.Contains(ward, StringComparison.OrdinalIgnoreCase));
            }

            // Filter theo Province
            if (!string.IsNullOrWhiteSpace(province))
            {
                complexes = complexes.Where(c => !string.IsNullOrEmpty(c.Province) &&
                    c.Province.Contains(province, StringComparison.OrdinalIgnoreCase));
            }

            // Filter theo SurfaceType (từ Fields)
            if (!string.IsNullOrWhiteSpace(surfaceType))
            {
                complexes = complexes.Where(c =>
                    c.Fields != null && c.Fields.Any(f =>
                        !string.IsNullOrEmpty(f.SurfaceType) &&
                        f.SurfaceType.Contains(surfaceType, StringComparison.OrdinalIgnoreCase)));
            }

            // Filter theo FieldSize (từ Fields)
            if (!string.IsNullOrWhiteSpace(fieldSize))
            {
                complexes = complexes.Where(c =>
                    c.Fields != null && c.Fields.Any(f =>
                        !string.IsNullOrEmpty(f.FieldSize) &&
                        f.FieldSize.Contains(fieldSize, StringComparison.OrdinalIgnoreCase)));
            }

            // Filter theo giá (từ TimeSlots)
            if (minPrice.HasValue || maxPrice.HasValue)
            {
                complexes = complexes.Where(c =>
                    c.Fields != null && c.Fields.Any(f =>
                        f.TimeSlots != null && f.TimeSlots.Any(ts =>
                            (!minPrice.HasValue || ts.Price >= minPrice.Value) &&
                            (!maxPrice.HasValue || ts.Price <= maxPrice.Value)
                        )
                    )
                );
            }

            // Filter theo rating (chỉ tính reviews visible và chưa bị xóa)
            // Lấy reviews từ Fields -> Bookings -> Reviews
            if (minRating.HasValue || maxRating.HasValue)
            {
                complexes = complexes.Where(c =>
                {
                    var complexReviews = c.Fields?
                        .SelectMany(f => f.Bookings ?? Enumerable.Empty<BookingEntity>())
                        .SelectMany(b => b.Reviews ?? Enumerable.Empty<ReviewEntity>())
                        .Where(r => r.IsVisible && !r.IsDeleted)
                        .ToList();

                    return complexReviews != null &&
                           complexReviews.Any() &&
                           (!minRating.HasValue || complexReviews.Average(r => r.Rating) >= minRating.Value) &&
                           (!maxRating.HasValue || complexReviews.Average(r => r.Rating) <= maxRating.Value);
                });
            }

            return _mapper.Map<IEnumerable<ComplexDto>>(complexes);
        }

        public async Task<ComplexDto> CreateComplexAsync(CreateComplexDto createComplexDto)
        {
            var complex = _mapper.Map<ComplexEntity>(createComplexDto);
            // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            var created = await _complexRepository.AddAsync(complex);

            _logger.LogInformation("Tạo complex mới thành công - Complex ID: {ComplexId}, Name: {Name}, Owner ID: {OwnerId}",
                created.Id, created.Name, created.OwnerId);

            return _mapper.Map<ComplexDto>(created);
        }

        public async Task<ComplexDto> CreateComplexByOwnerAsync(CreateComplexByOwnerDto createComplexDto, int ownerId)
        {
            var complex = _mapper.Map<ComplexEntity>(createComplexDto);
            complex.OwnerId = ownerId;
            // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            var created = await _complexRepository.AddAsync(complex);

            _logger.LogInformation("Owner tạo complex mới - Complex ID: {ComplexId}, Name: {Name}, Owner ID: {OwnerId}",
                created.Id, created.Name, ownerId);

            // Notify all admins about new complex pending approval
            try
            {
                var allUsers = await _userRepository.GetAllUsersWithRolesAsync();
                var admins = allUsers.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Admin")).ToList();

                foreach (var admin in admins)
                {
                    await _notificationService.CreateAndPushAsync(new NotificationEntity
                    {
                        UserId = admin.Id,
                        SenderId = ownerId,
                        Title = "Cụm sân mới cần phê duyệt",
                        Message = $"Cụm sân '{created.Name}' đã được tạo và đang chờ phê duyệt.",
                        Type = NotificationType.System,
                        RelatedTable = "COMPLEX",
                        RelatedId = created.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(7)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin notifications for new complex {ComplexId}", created.Id);
            }

            return _mapper.Map<ComplexDto>(created);
        }

        public async Task<ComplexDto> CreateComplexByAdminAsync(CreateComplexByAdminDto createComplexDto)
        {
            // Validate OwnerId phải tồn tại và có role Owner hoặc Admin
            var owner = await _userRepository.GetUserByIdWithRoleAsync(createComplexDto.OwnerId);

            if (owner == null)
                throw new Exception("Không tìm thấy Owner với ID này");

            // Check if user has Owner or Admin role via RBAC
            var hasOwnerRole = owner.UserRoles.Any(ur => ur.Role.Name == "Owner" || ur.Role.Name == "Admin");
            if (!hasOwnerRole)
                throw new Exception("User này không phải là Owner hoặc Admin, không thể tạo sân");

            var complex = _mapper.Map<ComplexEntity>(createComplexDto);
            // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            var created = await _complexRepository.AddAsync(complex);

            _logger.LogInformation("Admin tạo complex mới - Complex ID: {ComplexId}, Name: {Name}, Owner ID: {OwnerId}",
                created.Id, created.Name, created.OwnerId);

            return _mapper.Map<ComplexDto>(created);
        }

        public async Task UpdateComplexAsync(int id, UpdateComplexDto updateComplexDto)
        {
            var existingComplex = await _complexRepository.GetByIdAsync(id);
            if (existingComplex == null)
                throw new Exception("Complex not found");

            // Map các field cơ bản
            existingComplex.Name = updateComplexDto.Name;
            existingComplex.Street = updateComplexDto.Street;
            existingComplex.Ward = updateComplexDto.Ward;
            existingComplex.Province = updateComplexDto.Province;
            existingComplex.Phone = updateComplexDto.Phone;
            existingComplex.OpeningTime = updateComplexDto.OpeningTime;
            existingComplex.ClosingTime = updateComplexDto.ClosingTime;
            existingComplex.Description = updateComplexDto.Description;

            // Chỉ update Status và IsActive nếu được gửi lên (không null)
            if (updateComplexDto.Status.HasValue)
                existingComplex.Status = updateComplexDto.Status.Value;

            if (updateComplexDto.IsActive.HasValue)
                existingComplex.IsActive = updateComplexDto.IsActive.Value;

            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _complexRepository.UpdateAsync(existingComplex);

            _logger.LogInformation("Cập nhật complex - Complex ID: {ComplexId}, Name: {Name}",
                id, existingComplex.Name);
        }

        public async Task SoftDeleteComplexAsync(int id)
        {
            await _complexRepository.SoftDeleteAsync(id);

            _logger.LogWarning("Xóa mềm complex - Complex ID: {ComplexId}", id);
        }

        public async Task<bool> ToggleActiveAsync(int id, bool isActive)
        {
            var complex = await _complexRepository.GetByIdAsync(id);
            if (complex == null || complex.IsDeleted)
                return false;

            complex.IsActive = isActive;
            await _complexRepository.UpdateAsync(complex);
            _logger.LogInformation("Toggle isActive for ComplexId {ComplexId} to {IsActive}", id, isActive);
            return true;
        }

        public async Task ApproveComplexAsync(int id)
        {
            var complex = await _complexRepository.GetByIdAsync(id);
            if (complex == null)
                throw new Exception("Không tìm thấy sân");

            if (complex.Status != ComplexStatus.Pending)
                throw new Exception("Chỉ có thể phê duyệt sân đang ở trạng thái Pending");

            complex.Status = ComplexStatus.Approved;
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _complexRepository.UpdateAsync(complex);

            // Notify owner about approval
            try
            {
                await _notificationService.CreateAndPushAsync(new NotificationEntity
                {
                    UserId = complex.OwnerId,
                    Title = "Cụm sân đã được phê duyệt",
                    Message = $"Cụm sân '{complex.Name}' của bạn đã được phê duyệt và có thể hoạt động.",
                    Type = NotificationType.System,
                    RelatedTable = "COMPLEX",
                    RelatedId = complex.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(7)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send approval notification for complex {ComplexId}", id);
            }
        }

        public async Task RejectComplexAsync(int id, string? reason = null)
        {
            var complex = await _complexRepository.GetByIdAsync(id);
            if (complex == null)
                throw new Exception("Không tìm thấy sân");

            if (complex.Status != ComplexStatus.Pending)
                throw new Exception("Chỉ có thể từ chối sân đang ở trạng thái Pending");

            complex.Status = ComplexStatus.Rejected;
            complex.RejectionReason = reason;
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _complexRepository.UpdateAsync(complex);

            // Notify owner about rejection with reason
            try
            {
                var message = $"Cụm sân '{complex.Name}' của bạn đã bị từ chối.";
                if (!string.IsNullOrEmpty(reason))
                {
                    message += $" Lý do: {reason}";
                }

                await _notificationService.CreateAndPushAsync(new NotificationEntity
                {
                    UserId = complex.OwnerId,
                    Title = "Cụm sân đã bị từ chối",
                    Message = message,
                    Type = NotificationType.System,
                    RelatedTable = "COMPLEX",
                    RelatedId = complex.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(7)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send rejection notification for complex {ComplexId}", id);
            }
        }

        public async Task ResubmitComplexAsync(int id)
        {
            var complex = await _complexRepository.GetByIdAsync(id);
            if (complex == null)
                throw new Exception("Không tìm thấy sân");

            if (complex.Status != ComplexStatus.Rejected)
                throw new Exception("Chỉ có thể gửi lại yêu cầu phê duyệt cho sân đã bị từ chối");

            complex.Status = ComplexStatus.Pending;
            complex.RejectionReason = null; // Clear rejection reason
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _complexRepository.UpdateAsync(complex);

            // Notify all admins about resubmission
            try
            {
                var allUsers = await _userRepository.GetAllUsersWithRolesAsync();
                var admins = allUsers.Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Admin")).ToList();

                foreach (var admin in admins)
                {
                    await _notificationService.CreateAndPushAsync(new NotificationEntity
                    {
                        UserId = admin.Id,
                        SenderId = complex.OwnerId,
                        Title = "Cụm sân được gửi lại phê duyệt",
                        Message = $"Cụm sân '{complex.Name}' đã được chỉnh sửa và gửi lại để phê duyệt.",
                        Type = NotificationType.System,
                        RelatedTable = "COMPLEX",
                        RelatedId = complex.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(7)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send resubmit notifications for complex {ComplexId}", id);
            }
        }

        public async Task<AvailabilityDto?> GetAvailabilityAsync(int complexId, DateOnly startDate, int days)
        {
            // 1. Lấy complex với fields và timeslots
            var complex = await _complexRepository.GetComplexWithFullDetailsAsync(complexId);
            if (complex == null) return null;

            // 2. Tính toán endDate
            var endDate = startDate.AddDays(days - 1);

            // 3. Lấy danh sách bookings trong khoảng thời gian
            var bookings = await _bookingRepository.GetBookingsForComplexAsync(complexId, startDate, endDate);

            // 4. Tạo dictionary để tra cứu bookings nhanh theo (date, fieldId, timeSlotId)
            var bookingLookup = bookings
                .GroupBy(b => DateOnly.FromDateTime(b.BookingDate))
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(b => (b.FieldId, b.TimeSlotId)).ToHashSet()
                );

            // 5. Lấy tất cả các unique time slots (startTime, endTime) từ tất cả fields
            var allTimeSlots = complex.Fields
                .Where(f => !f.IsDeleted && f.IsActive)
                .SelectMany(f => f.TimeSlots.Where(ts => ts.IsActive))
                .Select(ts => new { ts.StartTime, ts.EndTime })
                .Distinct()
                .OrderBy(ts => ts.StartTime)
                .ToList();

            // 6. Lấy danh sách tất cả fields active
            var activeFields = complex.Fields
                .Where(f => !f.IsDeleted && f.IsActive)
                .ToList();

            // 7. Build response
            var result = new AvailabilityDto
            {
                ComplexId = complexId,
                Days = new List<AvailabilityDayDto>()
            };

            var now = TimeZoneHelper.VietnamNow;

            for (int i = 0; i < days; i++)
            {
                var currentDate = startDate.AddDays(i);
                var currentDateTime = currentDate.ToDateTime(TimeOnly.MinValue);

                var dayDto = new AvailabilityDayDto
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    TimeSlots = new List<AvailabilityTimeSlotDto>()
                };

                // Lấy booked slots cho ngày này
                var bookedSlotsForDay = bookingLookup.ContainsKey(currentDate)
                    ? bookingLookup[currentDate]
                    : new HashSet<(int FieldId, int TimeSlotId)>();

                foreach (var timeSlot in allTimeSlots)
                {
                    var timeSlotDto = new AvailabilityTimeSlotDto
                    {
                        StartTime = timeSlot.StartTime.ToString(@"hh\:mm"),
                        EndTime = timeSlot.EndTime.ToString(@"hh\:mm"),
                        Fields = new List<AvailabilityFieldDto>()
                    };

                    // Kiểm tra xem slot này có nằm trong quá khứ không
                    var slotDateTime = currentDateTime.Add(timeSlot.StartTime);
                    var isPast = slotDateTime < now;

                    foreach (var field in activeFields)
                    {
                        // Tìm timeslot tương ứng trong field
                        var fieldTimeSlot = field.TimeSlots.FirstOrDefault(ts =>
                            ts.StartTime == timeSlot.StartTime &&
                            ts.EndTime == timeSlot.EndTime &&
                            ts.IsActive);

                        if (fieldTimeSlot != null)
                        {
                            var isBooked = bookedSlotsForDay.Contains((field.Id, fieldTimeSlot.Id));

                            timeSlotDto.Fields.Add(new AvailabilityFieldDto
                            {
                                FieldId = field.Id,
                                FieldName = field.Name,
                                FieldSize = field.FieldSize,
                                SurfaceType = field.SurfaceType,
                                TimeSlotId = fieldTimeSlot.Id,
                                Price = fieldTimeSlot.Price,
                                IsBooked = isBooked,
                                IsPast = isPast
                            });
                        }
                    }

                    // Chỉ thêm timeslot nếu có ít nhất 1 field có slot này
                    if (timeSlotDto.Fields.Any())
                    {
                        dayDto.TimeSlots.Add(timeSlotDto);
                    }
                }

                result.Days.Add(dayDto);
            }

            return result;
        }

        public async Task<ComplexDto> BulkSetupComplexAsync(BulkSetupComplexDto bulkSetupDto, int ownerId)
        {
            _logger.LogInformation($"[BulkSetup] Starting bulk setup for owner {ownerId}");

            // Validate owner role
            var isOwner = await ValidateOwnerRoleAsync(ownerId);
            if (!isOwner)
                throw new InvalidOperationException("Chỉ Owner mới có thể tạo cụm sân");

            // 1. Create Complex
            var complex = _mapper.Map<ComplexEntity>(bulkSetupDto.Complex);
            complex.OwnerId = ownerId;
            complex.Status = ComplexStatus.Pending;
            complex.IsActive = true;
            complex.IsDeleted = false;
            complex.CreatedAt = DateTime.Now;
            complex.UpdatedAt = DateTime.Now;

            await _complexRepository.AddAsync(complex);
            _logger.LogInformation($"[BulkSetup] Complex created with ID: {complex.Id}");

            // 2. Create Fields with TimeSlots
            foreach (var fieldDto in bulkSetupDto.Fields)
            {
                var field = new FieldEntity
                {
                    ComplexId = complex.Id,
                    Name = fieldDto.Name,
                    FieldSize = fieldDto.FieldType,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _fieldRepository.AddAsync(field);
                _logger.LogInformation($"[BulkSetup] Field created: {field.Name} (ID: {field.Id})");

                // 3. Create TimeSlots for this Field
                List<TimeSlot> timeSlotsToCreate = new();

                // Use custom timeslots if provided, otherwise use template
                if (fieldDto.CustomTimeSlots != null && fieldDto.CustomTimeSlots.Any())
                {
                    timeSlotsToCreate = fieldDto.CustomTimeSlots.Select(ts => new TimeSlot
                    {
                        FieldId = field.Id,
                        StartTime = ts.StartTime,
                        EndTime = ts.EndTime,
                        Price = ts.Price,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }).ToList();
                }
                else if (bulkSetupDto.ApplyTemplateToAllFields && bulkSetupDto.TimeSlotTemplate != null)
                {
                    timeSlotsToCreate = bulkSetupDto.TimeSlotTemplate.TimeSlots.Select(ts => new TimeSlot
                    {
                        FieldId = field.Id,
                        StartTime = ts.StartTime,
                        EndTime = ts.EndTime,
                        Price = ts.Price,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }).ToList();
                }

                if (timeSlotsToCreate.Any())
                {
                    await _timeSlotRepository.AddRangeAsync(timeSlotsToCreate);
                    _logger.LogInformation($"[BulkSetup] Created {timeSlotsToCreate.Count} timeslots for field {field.Name}");
                }
            }

            _logger.LogInformation($"[BulkSetup] Completed: Complex ID {complex.Id}, {bulkSetupDto.Fields.Count} fields created");

            return _mapper.Map<ComplexDto>(complex);
        }

        // Admin only - Get all complexes without filters (except IsDeleted)
        public async Task<(IEnumerable<ComplexDto> complexes, int totalCount)> GetAllComplexesForAdminAsync(int pageIndex, int pageSize)
        {
            var (complexes, totalCount) = await _complexRepository.GetAllComplexesForAdminAsync(pageIndex, pageSize);
            var complexDtos = _mapper.Map<IEnumerable<ComplexDto>>(complexes);
            return (complexDtos, totalCount);
        }

        // Admin only - Get complex detail with rating, review count, images, and fields
        public async Task<AdminComplexDetailDto?> GetComplexDetailForAdminAsync(int id)
        {
            // Get complex with fields (admin can view all, no bank info check)
            var complex = await _complexRepository.GetComplexWithFieldsForAdminAsync(id);
            if (complex == null) return null;

            // Get owner info
            var owner = await _userRepository.GetByIdAsync(complex.OwnerId);
            
            // Get rating and review count
            var averageRating = await _reviewRepository.GetAverageRatingByComplexIdAsync(id);
            var reviews = await _reviewRepository.GetByComplexIdAsync(id);
            var reviewCount = reviews.Count();

            // Get images
            var images = await _complexImageRepository.GetByComplexIdAsync(id);

            // Map to DTO
            var dto = _mapper.Map<AdminComplexDetailDto>(complex);
            dto.OwnerName = owner != null ? $"{owner.FirstName} {owner.LastName}" : "N/A";
            dto.OwnerEmail = owner?.Email ?? "N/A";
            dto.AverageRating = averageRating;
            dto.ReviewCount = reviewCount;
            dto.Images = _mapper.Map<IEnumerable<ComplexImageResponseDto>>(images);

            return dto;
        }
    }
}
