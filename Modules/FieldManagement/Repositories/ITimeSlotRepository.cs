using FootballField.API.Shared.Base;
using FootballField.API.Modules.FieldManagement.Entities;

namespace FootballField.API.Modules.FieldManagement.Repositories
{
    public interface ITimeSlotRepository : IGenericRepository<TimeSlot>
    {
        Task<IEnumerable<TimeSlot>> GetByFieldIdAsync(int fieldId);
        Task<(IEnumerable<TimeSlot> timeSlots, int totalCount)> GetByFieldIdPagedAsync(int fieldId, int pageIndex, int pageSize);
        Task<(IEnumerable<TimeSlot> timeSlots, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize);
        Task<(IEnumerable<TimeSlot> timeSlots, int totalCount)> GetByOwnerIdPagedWithFiltersAsync(
            int ownerId, 
            int pageIndex, 
            int pageSize,
            string? searchTerm = null,
            int? complexId = null,
            int? fieldId = null,
            bool? isActive = null);
        Task<IEnumerable<TimeSlot>> GetActiveTimeSlotsAsync(int fieldId);
        new Task AddRangeAsync(IEnumerable<TimeSlot> timeSlots);
    }
}
