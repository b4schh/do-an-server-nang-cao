using DoAn.Core.Application.DTOs.Complex;

namespace DoAn.Core.Application.Interfaces.Complex;

public interface IComplexService
{
    Task<IEnumerable<ComplexDto>> GetAllComplexesAsync();
    Task<(IEnumerable<ComplexDto> complexes, int totalCount)> GetPagedComplexesAsync(int pageIndex, int pageSize);
    Task<ComplexDto?> GetComplexByIdAsync(int id);
    Task<ComplexWithFieldsDto?> GetComplexWithFieldsAsync(int id);
    Task<IEnumerable<ComplexDto>> GetComplexesByOwnerIdAsync(int ownerId);
    Task<(IEnumerable<ComplexDto> complexes, int totalCount)> GetComplexesByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize);
    Task<bool> ValidateOwnerRoleAsync(int ownerId);
    Task<ComplexDto> CreateComplexAsync(CreateComplexDto createComplexDto);
    Task<ComplexDto> CreateComplexByOwnerAsync(CreateComplexByOwnerDto createComplexDto, int ownerId);
    Task<ComplexDto> CreateComplexByAdminAsync(CreateComplexByAdminDto createComplexDto);
    Task UpdateComplexAsync(int id, UpdateComplexDto updateComplexDto);
    Task SoftDeleteComplexAsync(int id);
    Task ApproveComplexAsync(int id);
    Task RejectComplexAsync(int id, string? reason = null);
    Task ResubmitComplexAsync(int id);
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
        string? ward,
        string? province,
        string? surfaceType = null,
        string? fieldSize = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        double? minRating = null,
        double? maxRating = null);

    Task<bool> ToggleActiveAsync(int id, bool isActive);

    /// <summary>
    /// Bulk setup: Create complex with multiple fields and timeslots in one transaction
    /// </summary>
    Task<ComplexDto> BulkSetupComplexAsync(BulkSetupComplexDto bulkSetupDto, int ownerId);

    /// <summary>
    /// Admin only - Get all complexes without filters (pending, approved, rejected)
    /// </summary>
    Task<(IEnumerable<ComplexDto> complexes, int totalCount)> GetAllComplexesForAdminAsync(int pageIndex, int pageSize);

    /// <summary>
    /// Admin only - Get complex detail with rating, review count, images, and fields
    /// </summary>
    Task<AdminComplexDetailDto?> GetComplexDetailForAdminAsync(int id);
}
