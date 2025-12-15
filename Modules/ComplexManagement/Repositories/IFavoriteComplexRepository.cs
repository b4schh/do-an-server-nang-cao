using FootballField.API.Modules.ComplexManagement.Entities;

namespace FootballField.API.Modules.ComplexManagement.Repositories;

public interface IFavoriteComplexRepository
{
    Task<FavoriteComplex?> GetByUserAndComplexAsync(int userId, int complexId);
    Task<IEnumerable<Complex>> GetUserFavoriteComplexesAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int complexId);
    Task<FavoriteComplex> AddFavoriteAsync(int userId, int complexId);
    Task<bool> RemoveFavoriteAsync(int userId, int complexId);
}
