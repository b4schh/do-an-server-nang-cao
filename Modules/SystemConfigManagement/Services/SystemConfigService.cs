using AutoMapper;
using FootballField.API.Modules.SystemConfigManagement.Dtos;
using FootballField.API.Modules.SystemConfigManagement.Repositories;
using FootballField.API.Shared.Utils;

namespace FootballField.API.Modules.SystemConfigManagement.Services
{
    public class SystemConfigService : ISystemConfigService
    {
        private readonly ISystemConfigRepository _repo;
        private readonly IMapper _mapper;

        public SystemConfigService(ISystemConfigRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SystemConfigDto>> GetAllConfigsAsync()
        {
            var configs = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<SystemConfigDto>>(configs);
        }

        public async Task<SystemConfigDto?> GetConfigByKeyAsync(string key)
        {
            var config = await _repo.GetByKeyAsync(key);
            return _mapper.Map<SystemConfigDto>(config);
        }

        public async Task<T?> GetConfigValueAsync<T>(string key)
        {
            var config = await _repo.GetByKeyAsync(key);
            if (config == null || string.IsNullOrEmpty(config.ConfigValue))
                return default;

            try
            {
                return (T)Convert.ChangeType(config.ConfigValue, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public async Task UpdateConfigAsync(string key, UpdateSystemConfigDto dto)
        {
            var config = await _repo.GetByKeyAsync(key);
            if (config == null)
                throw new Exception($"SystemConfig with key '{key}' not found");

            config.ConfigValue = dto.ConfigValue;
            config.UpdatedAt = TimeZoneHelper.VietnamNow;

            await _repo.UpdateAsync(config);
        }
    }
}
