using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IOwnerSettingRepository : IGenericRepository<OwnerSetting>
    {
        Task<OwnerSetting?> GetByOwnerIdAsync(int ownerId);
    }
}