using DoAn.Core.Application.Interfaces.Location;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.Location;

public class ProvinceRepository : GenericRepository<ProvinceEntity>, IProvinceRepository
{
    public ProvinceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ProvinceEntity?> GetByCodeAsync(int code)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<ProvinceEntity?> GetByCodeWithWardsAsync(int code)
    {
        return await _dbSet
            .Include(p => p.Wards)
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<IEnumerable<ProvinceEntity>> GetAllWithWardsAsync()
    {
        return await _dbSet
            .Include(p => p.Wards)
            .ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(int code)
    {
        return await _dbSet.AnyAsync(p => p.Code == code);
    }
}
