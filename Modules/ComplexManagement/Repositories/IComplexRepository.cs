using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Shared.Base;

namespace FootballField.API.Modules.ComplexManagement.Repositories
{
    public interface IComplexRepository : IGenericRepository<Complex>
    {
        Task<IEnumerable<Complex>> GetByOwnerIdAsync(int ownerId);
        Task<(IEnumerable<Complex> complexes, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize);
        Task<IEnumerable<Complex>> GetActiveComplexesAsync();
        Task<Complex?> GetComplexWithFieldsAsync(int complexId);
        Task<Complex?> GetComplexWithFullDetailsAsync(int complexId);
        Task<IEnumerable<(Complex Complex, bool HasBankInfo)>> GetComplexesWithDetailsForSearchAsync();
    }
}
