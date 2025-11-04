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

        public async Task<TimeSlotDto> CreateTimeSlotAsync(CreateTimeSlotDto createTimeSlotDto)
        {
            var existingTimeSlots = await _timeSlotRepository.GetActiveTimeSlotsAsync(createTimeSlotDto.FieldId);

            foreach (var ts in existingTimeSlots)
            {
                if (IsOverlapping(createTimeSlotDto.StartTime, createTimeSlotDto.EndTime, ts.StartTime, ts.EndTime))
                    throw new Exception("Thời gian khung giờ bị trùng với một khung giờ khác của sân này.");
            }

            var timeSlot = _mapper.Map<TimeSlot>(createTimeSlotDto);
            timeSlot.CreatedAt = DateTime.Now;
            timeSlot.UpdatedAt = DateTime.Now;

            var created = await _timeSlotRepository.AddAsync(timeSlot);
            return _mapper.Map<TimeSlotDto>(created);
        }

        public async Task UpdateTimeSlotAsync(int id, UpdateTimeSlotDto updateTimeSlotDto)
        {
            var existingTimeSlot = await _timeSlotRepository.GetByIdAsync(id);
            if (existingTimeSlot == null)
                throw new Exception("TimeSlot not found");

            var existingTimeSlots = (await _timeSlotRepository.GetActiveTimeSlotsAsync(existingTimeSlot.FieldId))
                                    .Where(ts => ts.Id != id);

            foreach (var ts in existingTimeSlots)
            {
                if (IsOverlapping(updateTimeSlotDto.StartTime, updateTimeSlotDto.EndTime, ts.StartTime, ts.EndTime))
                    throw new Exception("Thời gian khung giờ bị trùng với một khung giờ khác của sân này.");
            }

            _mapper.Map(updateTimeSlotDto, existingTimeSlot);
            existingTimeSlot.UpdatedAt = DateTime.Now;

            await _timeSlotRepository.UpdateAsync(existingTimeSlot);
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
