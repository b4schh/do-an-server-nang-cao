using FootballField.API.Entities;
using FootballField.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using FootballField.API.DbContexts;

namespace FootballField.API.Repositories.Implements
{
    public class OwnerSettingRepository : GenericRepository<OwnerSetting>, IOwnerSettingRepository
    {
        private readonly ApplicationDbContext _context;

        public OwnerSettingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<OwnerSetting?> GetByOwnerIdAsync(int ownerId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.OwnerId == ownerId);
        }
    }
}
