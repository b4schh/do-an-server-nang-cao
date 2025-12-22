using FootballField.API.Modules.SystemConfigManagement.Dtos;

namespace FootballField.API.Modules.SystemConfigManagement.Services
{
    public interface ISystemConfigService
    {
        Task<IEnumerable<SystemConfigDto>> GetAllConfigsAsync();
        Task<SystemConfigDto?> GetConfigByKeyAsync(string key);
        Task<T?> GetConfigValueAsync<T>(string key);
        Task UpdateConfigAsync(string key, UpdateSystemConfigDto dto);
    }
}
