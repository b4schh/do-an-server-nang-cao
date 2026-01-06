using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DoAn.Infrastructure.Repositories.Complex;

public class ComplexRepository : GenericRepository<ComplexEntity>, IComplexRepository
{
    public ComplexRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<(IEnumerable<ComplexEntity> items, int totalCount)> GetPagedAsync(
        int pageIndex,
        int pageSize,
        Expression<Func<ComplexEntity, bool>>? filter = null)
    {
        // ✅ CRITICAL FIX: Build query WITHOUT loading to memory first
        // Get owner IDs that have bank account configured
        var ownerIdsWithBankQuery = _context.OwnerSettings
            .Where(os => !string.IsNullOrEmpty(os.BankAccountNumber))
            .Select(os => os.OwnerId);

        // Build the main query with all filters applied at database level
        var query = _dbSet
            .Include(c => c.ComplexImages)
            .Where(c => !c.IsDeleted 
                && c.Status == ComplexStatus.Approved 
                && c.IsActive
                && ownerIdsWithBankQuery.Contains(c.OwnerId));

        // Apply custom filter if provided
        if (filter != null)
            query = query.Where(filter);

        // ✅ Count at database level
        var totalCount = await query.CountAsync();

        // ✅ Apply pagination at database level
        var items = await query
            .OrderByDescending(c => c.CreatedAt) // Add ordering for consistent pagination
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<ComplexEntity>> GetByOwnerIdAsync(int ownerId)
    {
        return await _dbSet
            .Where(c => c.OwnerId == ownerId && !c.IsDeleted)
            .ToListAsync();
    }

    public async Task<(IEnumerable<ComplexEntity> complexes, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize)
    {
        var query = _dbSet
            .Include(c => c.Fields)
            .Where(c => c.OwnerId == ownerId && !c.IsDeleted);

        var totalCount = await query.CountAsync();
        var complexes = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (complexes, totalCount);
    }

    public async Task<IEnumerable<ComplexEntity>> GetActiveComplexesAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive && !c.IsDeleted && c.Status == ComplexStatus.Approved)
            .ToListAsync();
    }

    public async Task<ComplexEntity?> GetComplexWithFieldsAsync(int complexId)
    {
        var complex = await _dbSet
            .Include(c => c.Fields)
            .Include(c => c.ComplexImages)
            .FirstOrDefaultAsync(c => c.Id == complexId && !c.IsDeleted);

        if (complex == null)
            return null;

        // Kiểm tra bank info
        var hasBankInfo = await _context.OwnerSettings
            .AnyAsync(os => os.OwnerId == complex.OwnerId && !string.IsNullOrEmpty(os.BankAccountNumber));

        // Chỉ trả về nếu complex approved, active và có bank info
        if (complex.Status == ComplexStatus.Approved && complex.IsActive && hasBankInfo)
            return complex;

        return null;
    }

    // Admin only - Get complex with fields without bank info check
    public async Task<ComplexEntity?> GetComplexWithFieldsForAdminAsync(int complexId)
    {
        var complex = await _dbSet
            .Include(c => c.Fields.Where(f => !f.IsDeleted))
            .Include(c => c.ComplexImages)
            .FirstOrDefaultAsync(c => c.Id == complexId && !c.IsDeleted);

        return complex;
    }

    public async Task<ComplexEntity?> GetComplexWithFullDetailsAsync(int complexId)
    {
        var complex = await _dbSet
            .Include(c => c.Fields.Where(f => !f.IsDeleted && f.IsActive))
                .ThenInclude(f => f.TimeSlots.Where(ts => ts.IsActive))
            .Include(c => c.ComplexImages)
            .FirstOrDefaultAsync(c => c.Id == complexId && !c.IsDeleted);

        if (complex == null)
            return null;

        // Kiểm tra bank info
        var hasBankInfo = await _context.OwnerSettings
            .AnyAsync(os => os.OwnerId == complex.OwnerId && !string.IsNullOrEmpty(os.BankAccountNumber));

        // Chỉ trả về nếu complex approved, active và có bank info
        if (complex.Status == ComplexStatus.Approved && complex.IsActive && hasBankInfo)
            return complex;

        return null;
    }

    public async Task<IEnumerable<(ComplexEntity Complex, bool HasBankInfo)>> GetComplexesWithDetailsForSearchAsync()
    {
        // Load complexes với details
        var complexes = await _dbSet
            .Include(c => c.ComplexImages)
            .Include(c => c.Fields.Where(f => !f.IsDeleted))
                .ThenInclude(f => f.TimeSlots.Where(ts => ts.IsActive))
            .Include(c => c.Fields)
                .ThenInclude(f => f.Bookings)
                    .ThenInclude(b => b.Reviews.Where(r => !r.IsDeleted && r.IsVisible))
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        // Load owner settings
        var ownerIds = complexes.Select(c => c.OwnerId).Distinct().ToList();
        var ownerSettings = await _context.OwnerSettings
            .Where(os => ownerIds.Contains(os.OwnerId) && !string.IsNullOrEmpty(os.BankAccountNumber))
            .Select(os => os.OwnerId)
            .ToListAsync();

        // Map kết quả
        return complexes.Select(c => (c, ownerSettings.Contains(c.OwnerId)));
    }

    // Admin only - Get all complexes without filters (except IsDeleted)
    public async Task<(IEnumerable<ComplexEntity> complexes, int totalCount)> GetAllComplexesForAdminAsync(int pageIndex, int pageSize)
    {
        var query = _dbSet
            .Include(c => c.ComplexImages)
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();
        var complexes = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (complexes, totalCount);
    }

    #region Recommendation Methods

    /// <summary>
    /// Lấy Complex với đầy đủ thông tin cho recommendation
    /// QUAN TRỌNG: Chỉ lấy complex có owner đã cập nhật bank info
    /// </summary>
    public async Task<ComplexEntity?> GetComplexWithDetailsForRecommendationAsync(int complexId)
    {
        var complex = await _dbSet
            .Include(c => c.Fields.Where(f => !f.IsDeleted))
                .ThenInclude(f => f.TimeSlots)
            .Include(c => c.Fields)
                .ThenInclude(f => f.Bookings)
            .Include(c => c.ComplexImages)
            .FirstOrDefaultAsync(c => c.Id == complexId && !c.IsDeleted);

        if (complex == null)
            return null;

        // Kiểm tra bank info - chỉ trả về complex có owner đã cập nhật bank account
        var hasBankInfo = await _context.OwnerSettings
            .AnyAsync(os => os.OwnerId == complex.OwnerId && !string.IsNullOrEmpty(os.BankAccountNumber));

        // Chỉ trả về nếu complex approved, active và có bank info
        if (complex.Status == ComplexStatus.Approved && complex.IsActive && hasBankInfo)
            return complex;

        return null;
    }

    /// <summary>
    /// Lấy tất cả Complex active với đầy đủ thông tin
    /// QUAN TRỌNG: Chỉ lấy complex có owner đã cập nhật bank info
    /// OPTIMIZED: Filter at DB level, don't load Bookings for recommendation
    /// </summary>
    public async Task<IEnumerable<ComplexEntity>> GetAllActiveComplexesWithDetailsAsync(string? province = null)
    {
        // STEP 1: Lấy owner IDs có bank info (subquery)
        var ownerIdsWithBank = _context.OwnerSettings
            .Where(os => !string.IsNullOrEmpty(os.BankAccountNumber))
            .Select(os => os.OwnerId);

        // STEP 2: Build query với filters ở DB level
        var query = _dbSet
            .Include(c => c.Fields.Where(f => !f.IsDeleted))
                .ThenInclude(f => f.TimeSlots)
            .Include(c => c.ComplexImages)
            .Where(c => !c.IsDeleted 
                     && c.IsActive 
                     && c.Status == ComplexStatus.Approved
                     && ownerIdsWithBank.Contains(c.OwnerId));

        // STEP 3: Filter by province at DB level if provided
        if (!string.IsNullOrEmpty(province))
        {
            query = query.Where(c => c.Province == province);
        }

        // STEP 4: Execute query - only load what's needed
        var complexes = await query.ToListAsync();

        // STEP 5: Load booking counts separately for recommendation scoring
        // This is more efficient than loading all booking details
        var complexIds = complexes.Select(c => c.Id).ToList();
        var bookingCounts = await _context.Bookings
            .Where(b => complexIds.Contains(b.Field.ComplexId) && b.BookingStatus == BookingStatus.Completed)
            .GroupBy(b => b.Field.ComplexId)
            .Select(g => new { ComplexId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ComplexId, x => x.Count);

        // Attach booking counts to complexes for recommendation calculation
        foreach (var complex in complexes)
        {
            foreach (var field in complex.Fields)
            {
                // Set a virtual property or use a workaround
                // Since we can't add bookings, recommendation service should query counts separately
            }
        }

        return complexes;
    }

    #endregion
}

