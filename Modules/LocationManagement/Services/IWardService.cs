using FootballField.API.Modules.LocationManagement.Dtos;

namespace FootballField.API.Modules.LocationManagement.Services;

public interface IWardService
{
    Task<IEnumerable<WardDto>> GetWardsByProvinceCodeAsync(int provinceCode);
    Task<WardDto?> GetWardByCodeAsync(int code);
}
