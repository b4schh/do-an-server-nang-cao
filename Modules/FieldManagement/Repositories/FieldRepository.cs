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

        public async Task<(IEnumerable<Field> fields, int totalCount)> GetByComplexIdPagedAsync(int complexId, int pageIndex, int pageSize)
        {
            var query = _dbSet
                .Where(f => f.ComplexId == complexId && !f.IsDeleted);

            var totalCount = await query.CountAsync();
            var fields = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (fields, totalCount);
        }

        public async Task<(IEnumerable<Field> fields, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize)
        {
            var query = _dbSet
                .Include(f => f.Complex)
                .Where(f => f.Complex.OwnerId == ownerId && !f.IsDeleted);

            var totalCount = await query.CountAsync();
            var fields = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (fields, totalCount);
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
