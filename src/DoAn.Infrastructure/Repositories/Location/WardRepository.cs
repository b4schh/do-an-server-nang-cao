using DoAn.Core.Application.Interfaces.Location;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.Location;

public class WardRepository : GenericRepository<WardEntity>, IWardRepository
{
    public WardRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<WardEntity?> GetByCodeAsync(int code)
    {
        return await _dbSet
            .Include(w => w.Province)
            .FirstOrDefaultAsync(w => w.Code == code);
    }

    public async Task<IEnumerable<WardEntity>> GetByProvinceCodeAsync(int provinceCode)
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
