using AutoMapper;
using Microsoft.Extensions.Logging;

using FootballField.API.Modules.FieldManagement.Dtos;
using FootballField.API.Modules.FieldManagement.Entities;
using FootballField.API.Modules.FieldManagement.Repositories;

namespace FootballField.API.Modules.FieldManagement.Services
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TimeSlotService> _logger;

        public TimeSlotService(ITimeSlotRepository timeSlotRepository, IMapper mapper, ILogger<TimeSlotService> logger)
        {
            _timeSlotRepository = timeSlotRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TimeSlotDto>> GetAllTimeSlotsAsync()
        {
            var timeSlots = await _timeSlotRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TimeSlotDto>>(timeSlots);
        }

        public async Task<TimeSlotDto?> GetTimeSlotByIdAsync(int id)
        {
            var timeSlot = await _timeSlotRepository.GetByIdAsync(id);
            return timeSlot == null ? null : _mapper.Map<TimeSlotDto>(timeSlot);
        }

        public async Task<IEnumerable<TimeSlotDto>> GetTimeSlotsByFieldIdAsync(int fieldId)
        {
            var timeSlots = await _timeSlotRepository.GetActiveTimeSlotsAsync(fieldId);
            return _mapper.Map<IEnumerable<TimeSlotDto>>(timeSlots);
        }

        public async Task<(IEnumerable<TimeSlotDto> timeSlots, int totalCount)> GetTimeSlotsByFieldIdPagedAsync(int fieldId, int pageIndex, int pageSize)
        {
            var (timeSlots, totalCount) = await _timeSlotRepository.GetByFieldIdPagedAsync(fieldId, pageIndex, pageSize);
            var timeSlotDtos = _mapper.Map<IEnumerable<TimeSlotDto>>(timeSlots);
            return (timeSlotDtos, totalCount);
        }

        public async Task<(IEnumerable<TimeSlotDto> timeSlots, int totalCount)> GetTimeSlotsByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize)
        {
            var (timeSlots, totalCount) = await _timeSlotRepository.GetByOwnerIdPagedAsync(ownerId, pageIndex, pageSize);

            // Tạo dictionary để lookup nhanh O(1) thay vì FirstOrDefault O(n)
            var timeSlotDict = timeSlots.ToDictionary(ts => ts.Id);
            var timeSlotDtos = _mapper.Map<IEnumerable<TimeSlotDto>>(timeSlots).ToList();

            // Populate navigation properties với O(n) thay vì O(n²)
            foreach (var dto in timeSlotDtos)
            {
                if (timeSlotDict.TryGetValue(dto.Id, out var timeSlot) && timeSlot.Field != null)
                {
                    dto.FieldName = timeSlot.Field.Name;
                    dto.ComplexId = timeSlot.Field.ComplexId;
                    dto.ComplexName = timeSlot.Field.Complex?.Name;
                }
            }

            return (timeSlotDtos, totalCount);
        }

        private bool IsOverlapping(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && start2 < end1;
        }

        public async Task<(bool isSuccess, string? errorMessage, TimeSlotDto? data)> CreateTimeSlotAsync(CreateTimeSlotDto dto)
        {
            var existingTimeSlots = await _timeSlotRepository.GetActiveTimeSlotsAsync(dto.FieldId);

            foreach (var ts in existingTimeSlots)
            {
                if (IsOverlapping(dto.StartTime, dto.EndTime, ts.StartTime, ts.EndTime))
                    return (false, "Thời gian khung giờ bị trùng với một khung giờ khác của sân này.", null);
            }

            var timeSlot = _mapper.Map<TimeSlot>(dto);
            // Ensure Price is set from DTO (avoid any mapping issues)
            timeSlot.Price = dto.Price;
            // CreatedAt và UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            var created = await _timeSlotRepository.AddAsync(timeSlot);
            return (true, null, _mapper.Map<TimeSlotDto>(created));
        }

        public async Task<(bool isSuccess, string? errorMessage)> UpdateTimeSlotAsync(int id, UpdateTimeSlotDto dto)
        {
            var existingTimeSlot = await _timeSlotRepository.GetByIdAsync(id);
            if (existingTimeSlot == null)
                return (false, "Không tìm thấy khung giờ.");

            var existingTimeSlots = (await _timeSlotRepository.GetActiveTimeSlotsAsync(existingTimeSlot.FieldId))
                                    .Where(ts => ts.Id != id);

            foreach (var ts in existingTimeSlots)
            {
                if (IsOverlapping(dto.StartTime, dto.EndTime, ts.StartTime, ts.EndTime))
                    return (false, "Thời gian khung giờ bị trùng với một khung giờ khác của sân này.");
            }

            // Preserve existing IsActive and Price if DTO does not provide them.
            var previousIsActive = existingTimeSlot.IsActive;
            var previousPrice = existingTimeSlot.Price;
            _mapper.Map(dto, existingTimeSlot);
            if (!dto.IsActive.HasValue)
            {
                existingTimeSlot.IsActive = previousIsActive;
            }
            // If DTO provided Price, apply it; otherwise preserve previous price
            if (dto.Price.HasValue)
            {
                existingTimeSlot.Price = dto.Price.Value;
            }
            else
            {
                existingTimeSlot.Price = previousPrice;
            }
            // UpdatedAt sẽ được set bởi ApplicationDbContext.UpdateTimestamps()

            await _timeSlotRepository.UpdateAsync(existingTimeSlot);
            return (true, null);
        }

        public async Task<(bool isSuccess, string? errorMessage)> ToggleActiveAsync(int id, bool isActive)
        {
            var timeSlot = await _timeSlotRepository.GetByIdAsync(id);
            if (timeSlot == null)
                return (false, "Không tìm thấy khung giờ");
            timeSlot.IsActive = isActive;
            timeSlot.UpdatedAt = DateTime.UtcNow;
            await _timeSlotRepository.UpdateAsync(timeSlot);
            return (true, null);
        }
        
        public async Task DeleteTimeSlotAsync(int id)
        {
            var timeSlot = await _timeSlotRepository.GetByIdAsync(id);
            if (timeSlot != null)
            {
                await _timeSlotRepository.DeleteAsync(timeSlot);
            }
        }
    }
}