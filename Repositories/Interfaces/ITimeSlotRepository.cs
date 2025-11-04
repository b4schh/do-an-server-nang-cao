using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface ITimeSlotRepository : IGenericRepository<TimeSlot>
    {
        Task<IEnumerable<TimeSlot>> GetByFieldIdAsync(int fieldId);
        Task<IEnumerable<TimeSlot>> GetActiveTimeSlotsAsync(int fieldId);
    }
}
