using FootballField.API.Modules.OwnerSettingsManagement.Dtos;

namespace FootballField.API.Modules.OwnerSettingsManagement.Services
{
    public interface IOwnerSettingService
    {
        Task<IEnumerable<OwnerSettingDto>> GetAllAsync();
        Task<OwnerSettingDto?> GetByIdAsync(int id);
        Task<OwnerSettingDto?> GetByOwnerIdAsync(int ownerId);
        Task<OwnerSettingResponseDto> GetSettingsWithDefaultsAsync(int ownerId);
        Task<OwnerSettingDto> CreateAsync(CreateOwnerSettingDto dto);
        Task UpdateAsync(int id, UpdateOwnerSettingDto dto);
        Task UpdateSettingsAsync(int ownerId, UpdateOwnerSettingDto dto);
        Task UpdateBankInfoAsync(int ownerId, UpdateBankInfoDto dto);
        Task<bool> ValidateBankInfoAsync(int ownerId);
        Task DeleteAsync(int id);
    }
}