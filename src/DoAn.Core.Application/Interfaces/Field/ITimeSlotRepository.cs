using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.Field;

public interface ITimeSlotRepository : IGenericRepository<TimeSlotEntity>
{
    Task<IEnumerable<TimeSlotEntity>> GetByFieldIdAsync(int fieldId);
    Task<(IEnumerable<TimeSlotEntity> timeSlots, int totalCount)> GetByFieldIdPagedAsync(int fieldId, int pageIndex, int pageSize);
    Task<(IEnumerable<TimeSlotEntity> timeSlots, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize);
    Task<(IEnumerable<TimeSlotEntity> timeSlots, int totalCount)> GetByOwnerIdPagedWithFiltersAsync(
        int ownerId,
        int pageIndex,
        int pageSize,
        string? searchTerm = null,
        int? complexId = null,
        int? fieldId = null,
        bool? isActive = null);
    Task<IEnumerable<TimeSlotEntity>> GetActiveTimeSlotsAsync(int fieldId);
    new Task AddRangeAsync(IEnumerable<TimeSlotEntity> timeSlots);
}
