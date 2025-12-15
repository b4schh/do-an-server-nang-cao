using System.Security.Claims;
using FootballField.API.Modules.ComplexManagement.Dtos;
using FootballField.API.Modules.ComplexManagement.Services;
using FootballField.API.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Modules.ComplexManagement.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteComplexService _favoriteService;

    public FavoritesController(IFavoriteComplexService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    /// <summary>
    /// Get user's favorite complexes
    /// </summary>
    [HttpGet("my-favorites")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ComplexDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID in token");

        var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
        return Ok(ApiResponse<IEnumerable<ComplexDto>>.Ok(favorites, "Lấy danh sách sân yêu thích thành công"));
    }

    /// <summary>
    /// Check if a complex is in user's favorites
    /// </summary>
    [HttpGet("{complexId}/is-favorite")]
    [ProducesResponseType(typeof(ApiResponse<FavoriteStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IsFavorite(int complexId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID in token");

        var isFavorite = await _favoriteService.IsFavoriteAsync(userId, complexId);
        return Ok(ApiResponse<FavoriteStatusDto>.Ok(
            new FavoriteStatusDto { IsFavorite = isFavorite },
            "Kiểm tra trạng thái yêu thích thành công"));
    }

    /// <summary>
    /// Toggle favorite status (add/remove)
    /// </summary>
    [HttpPost("{complexId}/toggle")]
    [ProducesResponseType(typeof(ApiResponse<ToggleFavoriteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleFavorite(int complexId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID in token");

        var result = await _favoriteService.ToggleFavoriteAsync(userId, complexId);
        return Ok(ApiResponse<ToggleFavoriteResponseDto>.Ok(result, result.Message));
    }
}
