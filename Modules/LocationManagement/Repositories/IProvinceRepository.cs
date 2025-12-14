using FootballField.API.Modules.LocationManagement.Entities;
using FootballField.API.Shared.Base;

namespace FootballField.API.Modules.LocationManagement.Repositories;

public interface IProvinceRepository : IGenericRepository<Province>
{
    Task<Province?> GetByCodeAsync(int code);
    Task<Province?> GetByCodeWithWardsAsync(int code);
    Task<IEnumerable<Province>> GetAllWithWardsAsync();
    Task<bool> CodeExistsAsync(int code);
}
