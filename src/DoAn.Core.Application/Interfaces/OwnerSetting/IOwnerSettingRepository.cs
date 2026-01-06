using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.OwnerSetting;

public interface IOwnerSettingRepository : IGenericRepository<OwnerSettingEntity>
{
    Task<OwnerSettingEntity?> GetByOwnerIdAsync(int ownerId);
}