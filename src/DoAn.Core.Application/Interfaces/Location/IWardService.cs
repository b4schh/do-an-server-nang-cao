using DoAn.Core.Application.DTOs.Location;

namespace DoAn.Core.Application.Interfaces.Location;

public interface IWardService
{
    Task<IEnumerable<WardDto>> GetWardsByProvinceCodeAsync(int provinceCode);
    Task<WardDto?> GetWardByCodeAsync(int code);
}
