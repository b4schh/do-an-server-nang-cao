using DoAn.Core.Application.DTOs.Complex;

namespace DoAn.Core.Application.Interfaces.Complex;

public interface IFavoriteComplexService
{
    Task<IEnumerable<ComplexDto>> GetUserFavoritesAsync(int userId);
    Task<bool> IsFavoriteAsync(int userId, int complexId);
    Task<ToggleFavoriteResponseDto> ToggleFavoriteAsync(int userId, int complexId);
}
