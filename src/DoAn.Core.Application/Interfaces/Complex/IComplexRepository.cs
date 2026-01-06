using DoAn.Core.Application.Interfaces.Base;

namespace DoAn.Core.Application.Interfaces.Complex;

public interface IComplexRepository : IGenericRepository<ComplexEntity>
{
    Task<IEnumerable<ComplexEntity>> GetByOwnerIdAsync(int ownerId);
    Task<(IEnumerable<ComplexEntity> complexes, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize);
    Task<IEnumerable<ComplexEntity>> GetActiveComplexesAsync();
    Task<ComplexEntity?> GetComplexWithFieldsAsync(int complexId);
    Task<ComplexEntity?> GetComplexWithFullDetailsAsync(int complexId);
    Task<IEnumerable<(ComplexEntity Complex, bool HasBankInfo)>> GetComplexesWithDetailsForSearchAsync();
    
    // Admin only - Get all complexes without filters (except IsDeleted)
    Task<(IEnumerable<ComplexEntity> complexes, int totalCount)> GetAllComplexesForAdminAsync(int pageIndex, int pageSize);
    
    // Admin only - Get complex with fields without bank info check
    Task<ComplexEntity?> GetComplexWithFieldsForAdminAsync(int complexId);
    
    // Recommendation methods
    /// <summary>
    /// Lấy Complex với đầy đủ thông tin cho recommendation
    /// Include: Fields, TimeSlots, Bookings, ComplexImages
    /// </summary>
    Task<ComplexEntity?> GetComplexWithDetailsForRecommendationAsync(int complexId);
    
    /// <summary>
    /// Lấy tất cả Complex active với đầy đủ thông tin
    /// </summary>
    Task<IEnumerable<ComplexEntity>> GetAllActiveComplexesWithDetailsAsync(string? province = null);
}
