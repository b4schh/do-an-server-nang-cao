using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IComplexImageRepository : IGenericRepository<ComplexImage>
    {
        Task<List<ComplexImage>> GetByComplexIdAsync(int complexId);
    }
}
