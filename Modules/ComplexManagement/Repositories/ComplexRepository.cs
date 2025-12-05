using FootballField.API.Database;
using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FootballField.API.Modules.ComplexManagement.Repositories
{
    public class ComplexRepository : GenericRepository<Complex>, IComplexRepository
    {
        public ComplexRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<(IEnumerable<Complex> items, int totalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            Expression<Func<Complex, bool>>? filter = null)
        {
            IQueryable<Complex> query = _dbSet.Include(c => c.ComplexImages);

            if (filter != null)
                query = query.Where(filter);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<Complex>> GetByOwnerIdAsync(int ownerId)
        {
            return await _dbSet
                .Where(c => c.OwnerId == ownerId && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Complex>> GetActiveComplexesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive && !c.IsDeleted && c.Status == ComplexStatus.Approved)
                .ToListAsync();
        }

        public async Task<Complex?> GetComplexWithFieldsAsync(int complexId)
        {
            return await _dbSet
                .Include(c => c.Fields)
                .Include(c => c.ComplexImages)
                .FirstOrDefaultAsync(c => c.Id == complexId && !c.IsDeleted);
        }

        public async Task<Complex?> GetComplexWithFullDetailsAsync(int complexId)
        {
            return await _dbSet
                .Include(c => c.Fields.Where(f => !f.IsDeleted && f.IsActive))
                    .ThenInclude(f => f.TimeSlots.Where(ts => ts.IsActive))
                .Include(c => c.ComplexImages)
                .FirstOrDefaultAsync(c => c.Id == complexId && !c.IsDeleted);
        }

        public async Task<IEnumerable<Complex>> GetComplexesWithDetailsForSearchAsync()
        {
            return await _dbSet
                .Include(c => c.ComplexImages)
                .Include(c => c.Fields.Where(f => !f.IsDeleted))
                    .ThenInclude(f => f.TimeSlots.Where(ts => ts.IsActive))
                .Include(c => c.Fields)
                    .ThenInclude(f => f.Bookings)
                        .ThenInclude(b => b.Reviews.Where(r => !r.IsDeleted && r.IsVisible))
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }
    }
}
