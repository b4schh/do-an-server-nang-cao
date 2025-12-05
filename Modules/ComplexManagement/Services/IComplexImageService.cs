using FootballField.API.Modules.ComplexManagement.Dtos;

namespace FootballField.API.Modules.ComplexManagement.Services;

public interface IComplexImageService
{
    Task<ComplexImageResponseDto> CreateAsync(ComplexImageCreateDto dto);
    Task<ComplexImageResponseDto> GetByIdAsync(int id);
    Task<List<ComplexImageResponseDto>> GetByComplexIdAsync(int complexId);
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Upload ảnh cho complex với validation và lưu vào storage + database
    /// </summary>
    Task<ComplexImageResponseDto> UploadImageAsync(int complexId, IFormFile file, int userId, string? description = null);
    
    /// <summary>
    /// Lấy danh sách ảnh của complex với full URL
    /// </summary>
    Task<List<ComplexImageResponseDto>> GetImagesByComplexIdAsync(int complexId);
    
    /// <summary>
    /// Xóa ảnh từ storage và database
    /// </summary>
    Task DeleteImageAsync(int imageId, int userId);
}