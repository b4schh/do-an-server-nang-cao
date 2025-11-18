using FootballField.API.DbContexts;
using FootballField.API.Entities;
using FootballField.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Repositories.Implements
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
