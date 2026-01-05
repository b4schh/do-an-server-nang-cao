using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.Location;

public interface IProvinceRepository : IGenericRepository<Province>
{
    Task<Province?> GetByCodeAsync(int code);
    Task<Province?> GetByCodeWithWardsAsync(int code);
    Task<IEnumerable<Province>> GetAllWithWardsAsync();
    Task<bool> CodeExistsAsync(int code);
}
