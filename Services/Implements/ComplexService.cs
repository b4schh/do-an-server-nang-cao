using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.Complex;
using FootballField.API.Dtos.TimeSlot;
using FootballField.API.Dtos.Field;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Services.Interfaces;

namespace FootballField.API.Services.Implements
{
    public class ComplexService : IComplexService
    {
        private readonly IComplexRepository _complexRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public ComplexService(
            IComplexRepository complexRepository,
            IUserRepository userRepository,
            IBookingRepository bookingRepository,
            IMapper mapper)
        {
            _complexRepository = complexRepository;
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
            _mapper = mapper;
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

            var bookedTimeSlots = await _bookingRepository.GetBookedTimeSlotIdsForComplexAsync(id, date);

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
                    IsBooked = bookedTimeSlots.Contains(ts.Id)
                }).OrderBy(ts => ts.StartTime)
            });

            return complexDto;
        }

        public async Task<IEnumerable<ComplexDto>> GetComplexesByOwnerIdAsync(int ownerId)
        {
            var complexes = await _complexRepository.GetByOwnerIdAsync(ownerId);
            return _mapper.Map<IEnumerable<ComplexDto>>(complexes);
        }
        public async Task<IEnumerable<ComplexDto>> SearchComplexesAsync(
            string? name,
            string? street,
            string? ward,
            string? province,
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

            // Filter theo Street
            if (!string.IsNullOrWhiteSpace(street))
            {
                complexes = complexes.Where(c => !string.IsNullOrEmpty(c.Street) &&
                    c.Street.Contains(street, StringComparison.OrdinalIgnoreCase));
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
            if (minRating.HasValue || maxRating.HasValue)
            {
                complexes = complexes.Where(c =>
                    c.Reviews != null &&
                    c.Reviews.Any(r => r.IsVisible && !r.IsDeleted) &&
                    (!minRating.HasValue || c.Reviews.Where(r => r.IsVisible && !r.IsDeleted).Average(r => r.Rating) >= minRating.Value) &&
                    (!maxRating.HasValue || c.Reviews.Where(r => r.IsVisible && !r.IsDeleted).Average(r => r.Rating) <= maxRating.Value)
                );
            }

            return _mapper.Map<IEnumerable<ComplexDto>>(complexes);
        }

        public async Task<ComplexDto> CreateComplexAsync(CreateComplexDto createComplexDto)
        {
            var complex = _mapper.Map<Complex>(createComplexDto);
            complex.CreatedAt = DateTime.Now;
            complex.UpdatedAt = DateTime.Now;

            var created = await _complexRepository.AddAsync(complex);
            return _mapper.Map<ComplexDto>(created);
        }

        public async Task<ComplexDto> CreateComplexByOwnerAsync(CreateComplexByOwnerDto createComplexDto, int ownerId)
        {
            var complex = _mapper.Map<Complex>(createComplexDto);
            complex.OwnerId = ownerId;
            complex.CreatedAt = DateTime.Now;
            complex.UpdatedAt = DateTime.Now;

            var created = await _complexRepository.AddAsync(complex);
            return _mapper.Map<ComplexDto>(created);
        }

        public async Task<ComplexDto> CreateComplexByAdminAsync(CreateComplexByAdminDto createComplexDto)
        {
            // Validate OwnerId phải tồn tại và có role Owner
            var owner = await _userRepository.GetUserByIdWithRoleAsync(createComplexDto.OwnerId);

            if (owner == null)
                throw new Exception("Không tìm thấy Owner với ID này");

            if (owner.Role != UserRole.Owner && owner.Role != UserRole.Admin)
                throw new Exception("User này không phải là Owner hoặc Admin, không thể tạo sân");

            var complex = _mapper.Map<Complex>(createComplexDto);
            complex.CreatedAt = DateTime.Now;
            complex.UpdatedAt = DateTime.Now;

            var created = await _complexRepository.AddAsync(complex);
            return _mapper.Map<ComplexDto>(created);
        }

        public async Task UpdateComplexAsync(int id, UpdateComplexDto updateComplexDto)
        {
            var existingComplex = await _complexRepository.GetByIdAsync(id);
            if (existingComplex == null)
                throw new Exception("Complex not found");

            _mapper.Map(updateComplexDto, existingComplex);
            existingComplex.UpdatedAt = DateTime.Now;

            await _complexRepository.UpdateAsync(existingComplex);
        }

        public async Task SoftDeleteComplexAsync(int id)
        {
            await _complexRepository.SoftDeleteAsync(id);
        }

        public async Task ApproveComplexAsync(int id)
        {
            var complex = await _complexRepository.GetByIdAsync(id);
            if (complex == null)
                throw new Exception("Không tìm thấy sân");

            if (complex.Status != ComplexStatus.Pending)
                throw new Exception("Chỉ có thể phê duyệt sân đang ở trạng thái Pending");

            complex.Status = ComplexStatus.Approved;
            complex.UpdatedAt = DateTime.Now;

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
            complex.UpdatedAt = DateTime.Now;

            await _complexRepository.UpdateAsync(complex);
        }
    }
}
