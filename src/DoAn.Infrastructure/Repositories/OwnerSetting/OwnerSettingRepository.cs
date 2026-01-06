
using DoAn.Core.Application.Interfaces.OwnerSetting;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.OwnerSetting;

public class OwnerSettingRepository : GenericRepository<OwnerSettingEntity>, IOwnerSettingRepository
{
    public OwnerSettingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<OwnerSettingEntity?> GetByOwnerIdAsync(int ownerId)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.OwnerId == ownerId);
    }
}
