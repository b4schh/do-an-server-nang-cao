using FootballField.API.Dtos.Complex;

namespace FootballField.API.Services.Interfaces
{
    public interface IComplexService
    {
        Task<IEnumerable<ComplexDto>> GetAllComplexesAsync();
        Task<(IEnumerable<ComplexDto> complexes, int totalCount)> GetPagedComplexesAsync(int pageIndex, int pageSize);
        Task<ComplexDto?> GetComplexByIdAsync(int id);
        Task<ComplexWithFieldsDto?> GetComplexWithFieldsAsync(int id);
        Task<IEnumerable<ComplexDto>> GetComplexesByOwnerIdAsync(int ownerId);
        Task<ComplexDto> CreateComplexAsync(CreateComplexDto createComplexDto);
        Task UpdateComplexAsync(int id, UpdateComplexDto updateComplexDto);
        Task SoftDeleteComplexAsync(int id);
    }
}
