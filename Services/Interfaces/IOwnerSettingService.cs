using FootballField.API.Dtos.OwnerSetting;

namespace FootballField.API.Services.Interfaces
{
    public interface IOwnerSettingService
    {
        Task<IEnumerable<OwnerSettingDto>> GetAllAsync();
        Task<OwnerSettingDto?> GetByIdAsync(int id);
        Task<OwnerSettingDto> CreateAsync(CreateOwnerSettingDto dto);
        Task UpdateAsync(int id, UpdateOwnerSettingDto dto);
        Task DeleteAsync(int id);
    }
}
