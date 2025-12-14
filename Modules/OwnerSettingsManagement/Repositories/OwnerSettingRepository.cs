
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;
using FootballField.API.Database;
using FootballField.API.Modules.OwnerSettingsManagement.Entities;

namespace FootballField.API.Modules.OwnerSettingsManagement.Repositories
{
    public class OwnerSettingRepository : GenericRepository<OwnerSetting>, IOwnerSettingRepository
    {
        public OwnerSettingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<OwnerSetting?> GetByOwnerIdAsync(int ownerId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.OwnerId == ownerId);
        }
    }
}