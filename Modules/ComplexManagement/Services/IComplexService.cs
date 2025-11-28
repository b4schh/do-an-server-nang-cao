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
        Task<bool> ValidateOwnerRoleAsync(int ownerId);
        Task<ComplexDto> CreateComplexAsync(CreateComplexDto createComplexDto);
        Task<ComplexDto> CreateComplexByOwnerAsync(CreateComplexByOwnerDto createComplexDto, int ownerId);
        Task<ComplexDto> CreateComplexByAdminAsync(CreateComplexByAdminDto createComplexDto);
        Task UpdateComplexAsync(int id, UpdateComplexDto updateComplexDto);
        Task SoftDeleteComplexAsync(int id);
        Task ApproveComplexAsync(int id);
        Task RejectComplexAsync(int id);
        Task<ComplexFullDetailsDto?> GetComplexWithFullDetailsAsync(int id, DateTime date);
        /// <summary>
        /// Lấy thông tin complex với availability của từng field theo từng ngày trong khoảng thời gian
        /// </summary>
        Task<ComplexWeeklyDetailsDto?> GetComplexWeeklyDetailsAsync(int id, DateTime startDate, DateTime endDate);
        /// <summary>
        /// Lấy availability của complex theo từng ngày trong khoảng thời gian (startDate + days)
        /// Trả về danh sách các time slot duy nhất với trạng thái availability của từng field
        /// </summary>
        Task<AvailabilityDto?> GetAvailabilityAsync(int complexId, DateOnly startDate, int days);
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
