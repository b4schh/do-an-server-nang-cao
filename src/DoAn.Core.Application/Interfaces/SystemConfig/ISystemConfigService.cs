using DoAn.Core.Application.DTOs.SystemConfig;

namespace DoAn.Core.Application.Interfaces.SystemConfig;

public interface ISystemConfigService
{
    Task<IEnumerable<SystemConfigDto>> GetAllConfigsAsync();
    Task<SystemConfigDto?> GetConfigByKeyAsync(string key);
    Task<T?> GetConfigValueAsync<T>(string key);
    Task UpdateConfigAsync(string key, UpdateSystemConfigDto dto);
}
