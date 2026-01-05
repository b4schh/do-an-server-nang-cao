using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Core.Application.Common.Utils;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.Complex;

public class FavoriteComplexRepository : GenericRepository<FavoriteComplexEntity>, IFavoriteComplexRepository
{
    public FavoriteComplexRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<FavoriteComplexEntity?> GetByUserAndComplexAsync(int userId, int complexId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(fc => fc.UserId == userId && fc.ComplexId == complexId);
    }

    public async Task<IEnumerable<ComplexEntity>> GetUserFavoriteComplexesAsync(int userId)
    {
        return await _context.FavoriteComplexes
            .Where(fc => fc.UserId == userId)
            .Include(fc => fc.Complex)
                .ThenInclude(c => c.ComplexImages)
            .Select(fc => fc.Complex)
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsFavoriteAsync(int userId, int complexId)
    {
        return await _dbSet
            .AnyAsync(fc => fc.UserId == userId && fc.ComplexId == complexId);
    }

    public async Task<FavoriteComplexEntity> AddFavoriteAsync(int userId, int complexId)
    {
        var favorite = new FavoriteComplexEntity
        {
            UserId = userId,
            ComplexId = complexId,
            CreatedAt = TimeZoneHelper.VietnamNow
        };

        return await AddAsync(favorite);
    }

    public async Task<bool> RemoveFavoriteAsync(int userId, int complexId)
    {
        var favorite = await GetByUserAndComplexAsync(userId, complexId);
        if (favorite == null)
            return false;

        await DeleteAsync(favorite);
        return true;
    }
}
