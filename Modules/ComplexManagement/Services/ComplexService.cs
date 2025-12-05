using AutoMapper;

using FootballField.API.Modules.ComplexManagement.Dtos;
using FootballField.API.Modules.FieldManagement.Dtos;
using FootballField.API.Modules.ComplexManagement.Repositories;
using FootballField.API.Modules.BookingManagement.Repositories;
using FootballField.API.Modules.UserManagement.Repositories;
using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Shared.Utils;

namespace FootballField.API.Modules.ComplexManagement.Services
{
    public class ComplexService : IComplexService
    {
        private readonly IComplexRepository _complexRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ComplexService> _logger;

        public ComplexService(
            IComplexRepository complexRepository,
            IUserRepository userRepository,
            IBookingRepository bookingRepository,
            IMapper mapper,
            ILogger<ComplexService> logger)
        {
            _complexRepository = complexRepository;
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
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
            // Lấy tất cả complexes kèm Fields, TimeSlots, Reviews
            var complexes = await _complexRepository.GetComplexesWithDetailsForSearchAsync();

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
                        .SelectMany(f => f.Bookings ?? Enumerable.Empty<FootballField.API.Modules.BookingManagement.Entities.Booking>())
                        .SelectMany(b => b.Reviews ?? Enumerable.Empty<FootballField.API.Modules.ReviewManagement.Entities.Review>())
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
            var complex = _mapper.Map<Complex>(createComplexDto);
            // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            var created = await _complexRepository.AddAsync(complex);
            
            _logger.LogInformation("Tạo complex mới thành công - Complex ID: {ComplexId}, Name: {Name}, Owner ID: {OwnerId}",
                created.Id, created.Name, created.OwnerId);
            
            return _mapper.Map<ComplexDto>(created);
        }

        public async Task<ComplexDto> CreateComplexByOwnerAsync(CreateComplexByOwnerDto createComplexDto, int ownerId)
        {
            var complex = _mapper.Map<Complex>(createComplexDto);
            complex.OwnerId = ownerId;
            // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            var created = await _complexRepository.AddAsync(complex);
            
            _logger.LogInformation("Owner tạo complex mới - Complex ID: {ComplexId}, Name: {Name}, Owner ID: {OwnerId}",
                created.Id, created.Name, ownerId);
            
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

            var complex = _mapper.Map<Complex>(createComplexDto);
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

            _mapper.Map(updateComplexDto, existingComplex);
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
        }

        public async Task RejectComplexAsync(int id)
        {
            var complex = await _complexRepository.GetByIdAsync(id);
            if (complex == null)
                throw new Exception("Không tìm thấy sân");

            if (complex.Status != ComplexStatus.Pending)
                throw new Exception("Chỉ có thể từ chối sân đang ở trạng thái Pending");

            complex.Status = ComplexStatus.Rejected;
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _complexRepository.UpdateAsync(complex);
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
    }
}
