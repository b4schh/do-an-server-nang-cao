using FootballField.API.Modules.FieldManagement.Dtos;

namespace FootballField.API.Modules.FieldManagement.Services
{
    public interface IFieldService
    {
        Task<IEnumerable<FieldDto>> GetAllFieldsAsync();

        Task<List<FieldDto>> GetFieByComplexIdAsync(int complexId);
        Task<(IEnumerable<FieldDto> fields, int totalCount)> GetPagedFieldsAsync(int pageIndex, int pageSize);

      //  Task<IEnumerable<FieldDto>> SearchFieldByDistrictAsync(string district);
        Task<FieldDto?> GetFieldByIdAsync(int id);
        Task<FieldWithTimeSlotsDto?> GetFieldWithTimeSlotsAsync(int id);
        Task<IEnumerable<FieldDto>> GetFieldsByComplexIdAsync(int complexId);
        Task<FieldDto> CreateFieldAsync(CreateFieldDto createFieldDto);
        Task UpdateFieldAsync(int id, UpdateFieldDto updateFieldDto);
        Task SoftDeleteFieldAsync(int id);
    }
}
