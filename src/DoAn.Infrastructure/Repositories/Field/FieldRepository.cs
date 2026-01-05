using DoAn.Core.Application.Interfaces.Field;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.Field;

public class FieldRepository : GenericRepository<FieldEntity>, IFieldRepository
{
    public FieldRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FieldEntity>> GetByComplexIdAsync(int complexId)
    {
        return await _dbSet
            .Where(f => f.ComplexId == complexId && !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<(IEnumerable<FieldEntity> fields, int totalCount)> GetByComplexIdPagedAsync(int complexId, int pageIndex, int pageSize)
    {
        var query = _dbSet
            .Where(f => f.ComplexId == complexId && !f.IsDeleted);

        var totalCount = await query.CountAsync();
        var fields = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (fields, totalCount);
    }

    public async Task<(IEnumerable<FieldEntity> fields, int totalCount)> GetByOwnerIdPagedAsync(int ownerId, int pageIndex, int pageSize)
    {
        var query = _dbSet
            .Include(f => f.Complex)
            .Where(f => f.Complex.OwnerId == ownerId && !f.IsDeleted);

        var totalCount = await query.CountAsync();
        var fields = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (fields, totalCount);
    }

    public async Task<IEnumerable<FieldEntity>> GetActiveFieldsAsync()
    {
        return await _dbSet
            .Where(f => f.IsActive && !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<FieldEntity?> GetFieldWithTimeSlotsAsync(int fieldId)
    {
        return await _dbSet
            .Include(f => f.TimeSlots)
            .Include(f => f.Complex)
            .FirstOrDefaultAsync(f => f.Id == fieldId && !f.IsDeleted);
    }

    public async Task<FieldEntity?> GetFieldWithComplexAsync(int fieldId)
    {
        return await _dbSet
            .Include(f => f.Complex)
            .FirstOrDefaultAsync(f => f.Id == fieldId && !f.IsDeleted);
    }

    public async Task<IEnumerable<FieldEntity>> GetFieldsByOwnerIdAsync(int ownerId)
    {
        return await _dbSet
            .Include(f => f.Complex)
            .Where(f => f.Complex.OwnerId == ownerId && !f.IsDeleted)
            .ToListAsync();
    }


    public async Task<(IEnumerable<FieldEntity> fields, int totalCount)> GetByOwnerIdPagedWithFiltersAsync(
        int ownerId, int pageIndex, int pageSize,
        string? searchTerm, int? complexId, string? fieldSize, string? surfaceType, bool? isActive)
    {
        var query = _dbSet
            .Include(f => f.Complex)
            .Where(f => f.Complex.OwnerId == ownerId && !f.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(f => f.Name.ToLower().Contains(lowerSearchTerm) ||
                                    f.Complex.Name.ToLower().Contains(lowerSearchTerm));
        }

        // Apply complex filter
        if (complexId.HasValue)
        {
            query = query.Where(f => f.ComplexId == complexId.Value);
        }

        // Apply field size filter
        if (!string.IsNullOrWhiteSpace(fieldSize))
        {
            query = query.Where(f => f.FieldSize == fieldSize);
        }

        // Apply surface type filter
        if (!string.IsNullOrWhiteSpace(surfaceType))
        {
            query = query.Where(f => f.SurfaceType == surfaceType);
        }

        // Apply active status filter
        if (isActive.HasValue)
        {
            query = query.Where(f => f.IsActive == isActive.Value);
        }


        // Order by complex name, then field name
        query = query.OrderBy(f => f.Complex.Name).ThenBy(f => f.Name);

        // Count total records after filtering
        var totalCount = await query.CountAsync();

        // Apply pagination
        var fields = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (fields, totalCount);
    }
}
