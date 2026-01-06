using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.Complex;

public interface IComplexRepository : IGenericRepository<ComplexEntity>
{
    Task<IEnumerable<ComplexEntity>> GetByOwnerIdAsync(int ownerId);
    Task<(IEnumerable<ComplexEntity> complexes, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize);
    Task<IEnumerable<ComplexEntity>> GetActiveComplexesAsync();
    Task<ComplexEntity?> GetComplexWithFieldsAsync(int complexId);
    Task<ComplexEntity?> GetComplexWithFullDetailsAsync(int complexId);
    Task<IEnumerable<(ComplexEntity Complex, bool HasBankInfo)>> GetComplexesWithDetailsForSearchAsync();
}
