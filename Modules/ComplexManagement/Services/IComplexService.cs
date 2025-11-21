using FootballField.API.Modules.ComplexManagement.Dtos;

namespace FootballField.API.Modules.ComplexManagement.Services
{
    public interface IComplexService
    {
        Task<IEnumerable<ComplexDto>> GetAllComplexesAsync();
        Task<(IEnumerable<ComplexDto> complexes, int totalCount)> GetPagedComplexesAsync(int pageIndex, int pageSize);
        Task<ComplexDto?> GetComplexByIdAsync(int id);
        Task<ComplexWithFieldsDto?> GetComplexWithFieldsAsync(int id);
        Task<IEnumerable<ComplexDto>> GetComplexesByOwnerIdAsync(int ownerId);
        Task<ComplexDto> CreateComplexAsync(CreateComplexDto createComplexDto);
        Task<ComplexDto> CreateComplexByOwnerAsync(CreateComplexByOwnerDto createComplexDto, int ownerId);
        Task<ComplexDto> CreateComplexByAdminAsync(CreateComplexByAdminDto createComplexDto);
        Task UpdateComplexAsync(int id, UpdateComplexDto updateComplexDto);
        Task SoftDeleteComplexAsync(int id);
        Task ApproveComplexAsync(int id);
        Task RejectComplexAsync(int id);
        Task<ComplexFullDetailsDto?> GetComplexWithFullDetailsAsync(int id, DateTime date);
        Task<IEnumerable<ComplexDto>> SearchComplexesAsync(
            string? name,
            string? street,
            string? ward, 
            string? province,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            double? minRating = null,
            double? maxRating = null);
    }
}
