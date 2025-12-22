using FootballField.API.Database;
using FootballField.API.Modules.SystemConfigManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.SystemConfigManagement.Repositories
{
    public class SystemConfigRepository : GenericRepository<SystemConfig>, ISystemConfigRepository
    {
        public SystemConfigRepository(ApplicationDbContext context) : base(context) { }

        public async Task<SystemConfig?> GetByKeyAsync(string key)
        {
            return await _context.SystemConfigs
                .FirstOrDefaultAsync(x => x.ConfigKey == key);
        }
    }
}
