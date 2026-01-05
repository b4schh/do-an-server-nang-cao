using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.Complex;

public class ComplexImageRepository : GenericRepository<ComplexImageEntity>, IComplexImageRepository
{
    public ComplexImageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<ComplexImageEntity>> GetByComplexIdAsync(int complexId)
    {
        return await _dbSet
            .Where(ci => ci.ComplexId == complexId)
            .ToListAsync();
    }
}
