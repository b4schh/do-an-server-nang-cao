using FootballField.API.Shared.Base;
using FootballField.API.Modules.ComplexManagement.Entities;

namespace FootballField.API.Modules.ComplexManagement.Repositories
{
    public interface IComplexImageRepository : IGenericRepository<ComplexImage>
    {
        Task<List<ComplexImage>> GetByComplexIdAsync(int complexId);
    }
}
