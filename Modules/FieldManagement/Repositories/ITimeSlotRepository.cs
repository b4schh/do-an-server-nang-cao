using FootballField.API.Shared.Base;
using FootballField.API.Modules.FieldManagement.Entities;

namespace FootballField.API.Modules.FieldManagement.Repositories
{
    public interface ITimeSlotRepository : IGenericRepository<TimeSlot>
    {
        Task<IEnumerable<TimeSlot>> GetByFieldIdAsync(int fieldId);
        Task<IEnumerable<TimeSlot>> GetActiveTimeSlotsAsync(int fieldId);
    }
}
