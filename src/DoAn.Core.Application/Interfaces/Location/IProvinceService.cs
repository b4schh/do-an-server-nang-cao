using DoAn.Core.Application.DTOs.Location;

namespace DoAn.Core.Application.Interfaces.Location;

public interface IProvinceService
{
    Task<IEnumerable<ProvinceDto>> GetAllProvincesAsync();
    Task<ProvinceDto?> GetProvinceByCodeAsync(int code);
    Task<ProvinceWithWardsDto?> GetProvinceWithWardsByCodeAsync(int code);
}
