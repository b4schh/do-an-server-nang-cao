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
    /// Upload nhiều ảnh cùng lúc, ảnh đầu tiên sẽ là main image
    /// </summary>
    Task<List<ComplexImageResponseDto>> UploadMultipleImagesAsync(int complexId, List<IFormFile> files, int userId);
    
    /// <summary>
    /// Lấy danh sách ảnh của complex với full URL
    /// </summary>
    Task<List<ComplexImageResponseDto>> GetImagesByComplexIdAsync(int complexId);
    
    /// <summary>
    /// Xóa ảnh từ storage và database
    /// </summary>
    Task DeleteImageAsync(int imageId, int userId);
    
    /// <summary>
    /// Đặt ảnh làm main image của complex
    /// </summary>
    Task SetMainImageAsync(int imageId, int userId);
}