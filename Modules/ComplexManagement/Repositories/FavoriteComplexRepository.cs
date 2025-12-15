using FootballField.API.Database;
using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.ComplexManagement.Repositories;

public class FavoriteComplexRepository : IFavoriteComplexRepository
{
    private readonly ApplicationDbContext _context;

    public FavoriteComplexRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FavoriteComplex?> GetByUserAndComplexAsync(int userId, int complexId)
    {
        return await _context.FavoriteComplexes
            .FirstOrDefaultAsync(fc => fc.UserId == userId && fc.ComplexId == complexId);
    }

    public async Task<IEnumerable<Complex>> GetUserFavoriteComplexesAsync(int userId)
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
        return await _context.FavoriteComplexes
            .AnyAsync(fc => fc.UserId == userId && fc.ComplexId == complexId);
    }

    public async Task<FavoriteComplex> AddFavoriteAsync(int userId, int complexId)
    {
        var favorite = new FavoriteComplex
        {
            UserId = userId,
            ComplexId = complexId,
            CreatedAt = TimeZoneHelper.VietnamNow
        };

        _context.FavoriteComplexes.Add(favorite);
        await _context.SaveChangesAsync();
        
        return favorite;
    }

    public async Task<bool> RemoveFavoriteAsync(int userId, int complexId)
    {
        var favorite = await GetByUserAndComplexAsync(userId, complexId);
        if (favorite == null)
            return false;

        _context.FavoriteComplexes.Remove(favorite);
        await _context.SaveChangesAsync();
        
        return true;
    }
}
