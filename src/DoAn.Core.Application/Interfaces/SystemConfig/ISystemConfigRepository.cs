using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.SystemConfig;

public interface ISystemConfigRepository : IGenericRepository<DoAn.Core.Domain.Entities.SystemConfig>
{
    Task<DoAn.Core.Domain.Entities.SystemConfig?> GetByKeyAsync(string key);
}
