using FootballField.API.DbContexts;
using FootballField.API.Entities;
using FootballField.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Repositories.Implements
{
    public class ComplexRepository : GenericRepository<Complex>, IComplexRepository
    {
        public ComplexRepository(ApplicationDbContext context) : base(context)
        {
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

<<<<<<< HEAD
        public async Task<Complex?> GetComplexWithFullDetailsAsync(int complexId)
        {
            return await _dbSet
                .Include(c => c.Fields.Where(f => !f.IsDeleted && f.IsActive))
                    .ThenInclude(f => f.TimeSlots.Where(ts => ts.IsActive))
                .Include(c => c.ComplexImages)
                .FirstOrDefaultAsync(c => c.Id == complexId && !c.IsDeleted);
        }
=======
      
>>>>>>> origin/Vu
    }
}
