using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.Location;

public interface IWardRepository : IGenericRepository<Ward>
{
    Task<Ward?> GetByCodeAsync(int code);
    Task<IEnumerable<Ward>> GetByProvinceCodeAsync(int provinceCode);
    Task<bool> CodeExistsAsync(int code);
}
