using FootballField.API.Modules.LocationManagement.Entities;
using FootballField.API.Shared.Base;

namespace FootballField.API.Modules.LocationManagement.Repositories;

public interface IWardRepository : IGenericRepository<Ward>
{
    Task<Ward?> GetByCodeAsync(int code);
    Task<IEnumerable<Ward>> GetByProvinceCodeAsync(int provinceCode);
    Task<bool> CodeExistsAsync(int code);
}
