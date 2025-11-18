using FootballField.API.DbContexts;
using FootballField.API.Entities;
using FootballField.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Repositories.Implements
{
    public class FieldRepository : GenericRepository<Field>, IFieldRepository
    {
        public FieldRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Field>> GetByComplexIdAsync(int complexId)
        {
            return await _dbSet
                .Where(f => f.ComplexId == complexId && !f.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Field>> GetActiveFieldsAsync()
        {
            return await _dbSet
                .Where(f => f.IsActive && !f.IsDeleted)
                .ToListAsync();
        }

        public async Task<Field?> GetFieldWithTimeSlotsAsync(int fieldId)
        {
            return await _dbSet
                .Include(f => f.TimeSlots)
                .Include(f => f.Complex)
                .FirstOrDefaultAsync(f => f.Id == fieldId && !f.IsDeleted);
        }

        public async Task<Field?> GetFieldWithComplexAsync(int fieldId)
        {
            return await _dbSet
                .Include(f => f.Complex)
                .FirstOrDefaultAsync(f => f.Id == fieldId && !f.IsDeleted);
        }
    }
}
