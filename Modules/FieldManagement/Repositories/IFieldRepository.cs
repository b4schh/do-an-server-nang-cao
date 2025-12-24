using FootballField.API.Shared.Base;
using FootballField.API.Modules.FieldManagement.Entities;

namespace FootballField.API.Modules.FieldManagement.Repositories
{
    public interface IFieldRepository : IGenericRepository<Field>
    {
        Task<IEnumerable<Field>> GetByComplexIdAsync(int complexId);
        Task<(IEnumerable<Field> fields, int totalCount)> GetByComplexIdPagedAsync(int complexId, int pageIndex, int pageSize);
        Task<(IEnumerable<Field> fields, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize);
        Task<(IEnumerable<Field> fields, int totalCount)> GetByOwnerIdPagedWithFiltersAsync(
            int ownerId, int pageIndex, int pageSize,
            string? searchTerm, int? complexId, string? fieldSize, string? surfaceType, bool? isActive);
        Task<IEnumerable<Field>> GetActiveFieldsAsync();
        Task<Field?> GetFieldWithTimeSlotsAsync(int fieldId);
        Task<Field?> GetFieldWithComplexAsync(int fieldId);
        Task<IEnumerable<Field>> GetFieldsByOwnerIdAsync(int ownerId);
    }
}
