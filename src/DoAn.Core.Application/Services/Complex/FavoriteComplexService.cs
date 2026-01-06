using AutoMapper;
using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Core.Application.DTOs.Complex;
using Microsoft.Extensions.Logging;

namespace DoAn.Core.Application.Services.Complex;

public class FavoriteComplexService : IFavoriteComplexService
{
    private readonly IFavoriteComplexRepository _favoriteRepository;
    private readonly IComplexRepository _complexRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FavoriteComplexService> _logger;

    public FavoriteComplexService(
        IFavoriteComplexRepository favoriteRepository,
        IComplexRepository complexRepository,
        IMapper mapper,
        ILogger<FavoriteComplexService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _complexRepository = complexRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ComplexDto>> GetUserFavoritesAsync(int userId)
    {
        var complexes = await _favoriteRepository.GetUserFavoriteComplexesAsync(userId);
        return _mapper.Map<IEnumerable<ComplexDto>>(complexes);
    }

    public async Task<bool> IsFavoriteAsync(int userId, int complexId)
    {
        return await _favoriteRepository.IsFavoriteAsync(userId, complexId);
    }

    public async Task<ToggleFavoriteResponseDto> ToggleFavoriteAsync(int userId, int complexId)
    {
        // Verify complex exists
        var complex = await _complexRepository.GetByIdAsync(complexId);
        if (complex == null)
            throw new KeyNotFoundException("Không tìm thấy sân");

        var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, complexId);

        if (isFavorite)
        {
            // Remove favorite
            await _favoriteRepository.RemoveFavoriteAsync(userId, complexId);
            _logger.LogInformation("User {UserId} removed complex {ComplexId} from favorites", userId, complexId);
            
            return new ToggleFavoriteResponseDto
            {
                IsFavorite = false,
                Message = "Đã xóa khỏi danh sách yêu thích"
            };
        }
        else
        {
            // Add favorite
            await _favoriteRepository.AddFavoriteAsync(userId, complexId);
            _logger.LogInformation("User {UserId} added complex {ComplexId} to favorites", userId, complexId);
            
            return new ToggleFavoriteResponseDto
            {
                IsFavorite = true,
                Message = "Đã thêm vào danh sách yêu thích"
            };
        }
    }
}
