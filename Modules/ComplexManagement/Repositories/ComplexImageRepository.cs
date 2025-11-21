using FootballField.API.Database;
using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Shared.Base;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.ComplexManagement.Repositories
{
    public class ComplexImageRepository : GenericRepository<ComplexImage>, IComplexImageRepository
    {
        public ComplexImageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<ComplexImage>> GetByComplexIdAsync(int complexId)
        {
            return await _dbSet
                .Where(ci => ci.ComplexId == complexId)
                .ToListAsync();
        }
    }
}
