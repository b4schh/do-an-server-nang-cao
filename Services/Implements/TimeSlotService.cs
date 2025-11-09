using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.TimeSlot;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Services.Interfaces;

namespace FootballField.API.Services.Implements
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly IMapper _mapper;

        public TimeSlotService(ITimeSlotRepository timeSlotRepository, IMapper mapper)
        {
            _timeSlotRepository = timeSlotRepository;
            _mapper = mapper;
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
            timeSlot.CreatedAt = DateTime.Now;
            timeSlot.UpdatedAt = DateTime.Now;

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

            _mapper.Map(dto, existingTimeSlot);
            existingTimeSlot.UpdatedAt = DateTime.Now;

            await _timeSlotRepository.UpdateAsync(existingTimeSlot);
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