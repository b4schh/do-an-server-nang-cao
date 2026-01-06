using DoAn.Core.Domain.Entities;

namespace DoAn.Core.Application.Interfaces.Complex;

public interface IFavoriteComplexRepository
{
    Task<FavoriteComplex?> GetByUserAndComplexAsync(int userId, int complexId);
    Task<IEnumerable<ComplexEntity>> GetUserFavoriteComplexesAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int complexId);
    Task<FavoriteComplex> AddFavoriteAsync(int userId, int complexId);
    Task<bool> RemoveFavoriteAsync(int userId, int complexId);
}
