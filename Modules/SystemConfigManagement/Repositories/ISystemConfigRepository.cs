using FootballField.API.Modules.SystemConfigManagement.Entities;
using FootballField.API.Shared.Base;

namespace FootballField.API.Modules.SystemConfigManagement.Repositories
{
    public interface ISystemConfigRepository : IGenericRepository<SystemConfig>
    {
        Task<SystemConfig?> GetByKeyAsync(string key);
    }
}
