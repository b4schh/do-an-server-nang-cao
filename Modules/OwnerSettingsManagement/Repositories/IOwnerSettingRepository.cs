

using FootballField.API.Modules.OwnerSettingsManagement.Entities;
using FootballField.API.Shared.Base;

namespace FootballField.API.Modules.OwnerSettingsManagement.Repositories
{
    public interface IOwnerSettingRepository : IGenericRepository<OwnerSetting>
    {
        Task<OwnerSetting?> GetByOwnerIdAsync(int ownerId);
    }
}