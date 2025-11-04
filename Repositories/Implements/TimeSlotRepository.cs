using FootballField.API.DbContexts;
using FootballField.API.Entities;
using FootballField.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Repositories.Implements
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