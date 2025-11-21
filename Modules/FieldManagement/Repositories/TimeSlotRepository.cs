using FootballField.API.Database;
using FootballField.API.Modules.FieldManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.FieldManagement.Repositories
{
    public class TimeSlotRepository : GenericRepository<TimeSlot>, ITimeSlotRepository
    {
        public TimeSlotRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TimeSlot>> GetByFieldIdAsync(int fieldId)
        {
            return await _dbSet
                .Where(ts => ts.FieldId == fieldId)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeSlot>> GetActiveTimeSlotsAsync(int fieldId)
        {
            return await _dbSet
                .Where(ts => ts.FieldId == fieldId && ts.IsActive)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();
        }
    }
}
