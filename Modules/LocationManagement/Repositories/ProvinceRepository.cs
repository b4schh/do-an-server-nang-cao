using FootballField.API.Database;
using FootballField.API.Modules.LocationManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.LocationManagement.Repositories;

public class ProvinceRepository : GenericRepository<Province>, IProvinceRepository
{
    public ProvinceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Province?> GetByCodeAsync(int code)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<Province?> GetByCodeWithWardsAsync(int code)
    {
        return await _dbSet
            .Include(p => p.Wards)
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<IEnumerable<Province>> GetAllWithWardsAsync()
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
