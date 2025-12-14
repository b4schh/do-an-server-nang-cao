using FootballField.API.Database;
using FootballField.API.Modules.LocationManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.LocationManagement.Repositories;

public class WardRepository : GenericRepository<Ward>, IWardRepository
{
    public WardRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Ward?> GetByCodeAsync(int code)
    {
        return await _dbSet
            .Include(w => w.Province)
            .FirstOrDefaultAsync(w => w.Code == code);
    }

    public async Task<IEnumerable<Ward>> GetByProvinceCodeAsync(int provinceCode)
    {
        return await _dbSet
            .Where(w => w.ProvinceCode == provinceCode)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(int code)
    {
        return await _dbSet.AnyAsync(w => w.Code == code);
    }
}
