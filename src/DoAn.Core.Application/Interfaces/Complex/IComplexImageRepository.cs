using DoAn.Core.Application.Interfaces.Base;
using DoAn.Core.Domain.Entities;

namespace DoAn.Core.Application.Interfaces.Complex;

public interface IComplexImageRepository : IGenericRepository<ComplexImage>
{
    Task<List<ComplexImage>> GetByComplexIdAsync(int complexId);
}
