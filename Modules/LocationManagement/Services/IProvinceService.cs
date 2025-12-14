using FootballField.API.Modules.LocationManagement.Dtos;

namespace FootballField.API.Modules.LocationManagement.Services;

public interface IProvinceService
{
    Task<IEnumerable<ProvinceDto>> GetAllProvincesAsync();
    Task<ProvinceDto?> GetProvinceByCodeAsync(int code);
    Task<ProvinceWithWardsDto?> GetProvinceWithWardsByCodeAsync(int code);
}
