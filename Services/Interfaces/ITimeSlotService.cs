using FootballField.API.Dtos.TimeSlot;

namespace FootballField.API.Services.Interfaces
{
    public interface ITimeSlotService
    {
        Task<IEnumerable<TimeSlotDto>> GetAllTimeSlotsAsync();
        Task<TimeSlotDto?> GetTimeSlotByIdAsync(int id);
        Task<IEnumerable<TimeSlotDto>> GetTimeSlotsByFieldIdAsync(int fieldId);
        Task<(bool isSuccess, string? errorMessage, TimeSlotDto? data)> CreateTimeSlotAsync(CreateTimeSlotDto createTimeSlotDto);
        Task<(bool isSuccess, string? errorMessage)> UpdateTimeSlotAsync(int id, UpdateTimeSlotDto updateTimeSlotDto);
        Task DeleteTimeSlotAsync(int id);
    }
}
