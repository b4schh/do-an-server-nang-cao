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

        public async Task<(IEnumerable<TimeSlot> timeSlots, int totalCount)> GetByFieldIdPagedAsync(int fieldId, int pageIndex, int pageSize)
        {
            var query = _dbSet
                .Where(ts => ts.FieldId == fieldId)
                .OrderBy(ts => ts.StartTime);

            var totalCount = await query.CountAsync();
            var timeSlots = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (timeSlots, totalCount);
        }

        public async Task<(IEnumerable<TimeSlot> timeSlots, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize)
        {
            var query = _dbSet
                .Include(ts => ts.Field)
                    .ThenInclude(f => f.Complex)
                .Where(ts => ts.Field.Complex.OwnerId == ownerId)
                .OrderBy(ts => ts.Field.Complex.Name)      // Sắp xếp theo tên Complex
                .ThenBy(ts => ts.Field.Name)               // Sau đó theo tên Field
                .ThenBy(ts => ts.StartTime);               // Cuối cùng theo giờ bắt đầu

            var totalCount = await query.CountAsync();
            var timeSlots = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (timeSlots, totalCount);
        }

        public async Task<IEnumerable<TimeSlot>> GetActiveTimeSlotsAsync(int fieldId)
        {
            return await _dbSet
                .Where(ts => ts.FieldId == fieldId && ts.IsActive)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();
        }

        public new async Task AddRangeAsync(IEnumerable<TimeSlot> timeSlots)
        {
            await _dbSet.AddRangeAsync(timeSlots);
            await _context.SaveChangesAsync();
        }
    }
}
