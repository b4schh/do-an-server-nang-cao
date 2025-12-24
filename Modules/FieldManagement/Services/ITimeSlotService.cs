using FootballField.API.Modules.FieldManagement.Dtos;

namespace FootballField.API.Modules.FieldManagement.Services
{
    public interface ITimeSlotService
    {
        Task<IEnumerable<TimeSlotDto>> GetAllTimeSlotsAsync();
        Task<TimeSlotDto?> GetTimeSlotByIdAsync(int id);
        Task<IEnumerable<TimeSlotDto>> GetTimeSlotsByFieldIdAsync(int fieldId);
        Task<(IEnumerable<TimeSlotDto> timeSlots, int totalCount)> GetTimeSlotsByFieldIdPagedAsync(int fieldId, int pageIndex, int pageSize);
        Task<(IEnumerable<TimeSlotDto> timeSlots, int totalCount)> GetTimeSlotsByOwnerIdPagedAsync(
            int ownerId, 
            int pageIndex, 
            int pageSize,
            string? searchTerm = null,
            int? complexId = null,
            int? fieldId = null,
            bool? isActive = null);
        Task<(bool isSuccess, string? errorMessage, TimeSlotDto? data)> CreateTimeSlotAsync(CreateTimeSlotDto createTimeSlotDto);
        Task<(bool isSuccess, string? errorMessage)> UpdateTimeSlotAsync(int id, UpdateTimeSlotDto updateTimeSlotDto);
        Task DeleteTimeSlotAsync(int id);
        Task<(bool isSuccess, string? errorMessage)> ToggleActiveAsync(int id, bool isActive);
    }
}
