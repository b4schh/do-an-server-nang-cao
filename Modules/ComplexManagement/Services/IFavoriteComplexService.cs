using FootballField.API.Modules.ComplexManagement.Dtos;

namespace FootballField.API.Modules.ComplexManagement.Services;

public interface IFavoriteComplexService
{
    Task<IEnumerable<ComplexDto>> GetUserFavoritesAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int complexId);
    Task<ToggleFavoriteResponseDto> ToggleFavoriteAsync(int userId, int complexId);
}
