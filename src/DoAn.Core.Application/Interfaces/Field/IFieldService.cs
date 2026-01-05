using DoAn.Core.Application.DTOs.Field;

namespace DoAn.Core.Application.Interfaces.Field;

public interface IFieldService
{
    Task<IEnumerable<FieldDto>> GetAllFieldsAsync();
    Task<(IEnumerable<FieldDto> fields, int totalCount)> GetPagedFieldsAsync(int pageIndex, int pageSize);
    Task<FieldDto?> GetFieldByIdAsync(int id);
    Task<FieldWithTimeSlotsDto?> GetFieldWithTimeSlotsAsync(int id);
    Task<IEnumerable<FieldDto>> GetFieldsByComplexIdAsync(int complexId);
    Task<(IEnumerable<FieldDto> fields, int totalCount)> GetFieldsByComplexIdPagedAsync(int complexId, int pageIndex, int pageSize, bool includeTimeSlotCount = false);
    Task<(IEnumerable<FieldDto> fields, int totalCount)> GetFieldsByComplexIdWithTimeSlotCountAsync(int complexId, int pageIndex, int pageSize);
    Task<(IEnumerable<FieldDto> fields, int totalCount)> GetFieldsByOwnerIdPagedAsync(
        int ownerId, int pageIndex, int pageSize,
        string? searchTerm = null, int? complexId = null, string? fieldSize = null, string? surfaceType = null, bool? isActive = null);
    Task<FieldDto> CreateFieldAsync(CreateFieldDto createFieldDto);
    Task UpdateFieldAsync(int id, UpdateFieldDto updateFieldDto);
    Task<bool> ToggleActiveAsync(int id, bool isActive);
    Task SoftDeleteFieldAsync(int id);

    // Bulk operations
    Task<FieldDto> CloneFieldAsync(int fieldId, CloneFieldDto cloneFieldDto);
    Task BatchAddTimeSlotsAsync(BatchAddTimeSlotsDto batchAddTimeSlotsDto);
}
