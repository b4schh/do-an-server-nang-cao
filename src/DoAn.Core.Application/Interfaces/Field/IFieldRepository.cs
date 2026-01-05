using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.Field;

public interface IFieldRepository : IGenericRepository<FieldEntity>
{
    Task<IEnumerable<FieldEntity>> GetByComplexIdAsync(int complexId);
    Task<(IEnumerable<FieldEntity> fields, int totalCount)> GetByComplexIdPagedAsync(int complexId, int pageIndex, int pageSize);
    Task<(IEnumerable<FieldEntity> fields, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize);
    Task<(IEnumerable<FieldEntity> fields, int totalCount)> GetByOwnerIdPagedWithFiltersAsync(
        int ownerId, int pageIndex, int pageSize,
        string? searchTerm, int? complexId, string? fieldSize, string? surfaceType, bool? isActive);
    Task<IEnumerable<FieldEntity>> GetActiveFieldsAsync();
    Task<FieldEntity?> GetFieldWithTimeSlotsAsync(int fieldId);
    Task<FieldEntity?> GetFieldWithComplexAsync(int fieldId);
    Task<IEnumerable<FieldEntity>> GetFieldsByOwnerIdAsync(int ownerId);
}
