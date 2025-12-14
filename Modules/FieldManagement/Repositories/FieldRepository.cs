using FootballField.API.Database;
using FootballField.API.Modules.FieldManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.FieldManagement.Repositories
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

        public IQueryable<Field> Query()
    {
        return _context.Fields
            .Include(f => f.Complex)          // để lấy District, Address, Parking...
            .AsNoTracking();                  // query đọc, không cần tracking
    }

 
    


     public async Task<List<Field>> GetFieldByComplexIdAsync(int complexId)
        {
            return await _dbSet
                .Where(f => f.ComplexId == complexId && !f.IsDeleted)
                .ToListAsync();
        }



    }
}
