using FootballField.API.Entities;

namespace FootballField.API.Repositories.Interfaces
{
    public interface IOwnerSettingRepository : IGenericRepository<OwnerSetting>
    {
        // Nếu có thêm hàm đặc thù thì khai báo ở đây
        Task<OwnerSetting?> GetByOwnerIdAsync(int ownerId);
    }
}
