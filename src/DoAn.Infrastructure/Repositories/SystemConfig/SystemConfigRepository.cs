using DoAn.Core.Application.Interfaces.SystemConfig;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.SystemConfig;

public class SystemConfigRepository : GenericRepository<DoAn.Core.Domain.Entities.SystemConfig>, ISystemConfigRepository
{
    public SystemConfigRepository(ApplicationDbContext context) : base(context) { }

    public async Task<DoAn.Core.Domain.Entities.SystemConfig?> GetByKeyAsync(string key)
    {
        return await _context.SystemConfigs
            .FirstOrDefaultAsync(x => x.ConfigKey == key);
    }
}

