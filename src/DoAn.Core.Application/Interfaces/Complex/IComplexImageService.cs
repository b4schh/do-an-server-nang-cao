using DoAn.Core.Application.DTOs.Complex;
using Microsoft.AspNetCore.Http;

namespace DoAn.Core.Application.Interfaces.Complex;

public interface IComplexImageService
{
    Task<ComplexImageResponseDto> CreateAsync(ComplexImageCreateDto dto);
    Task<ComplexImageResponseDto> GetByIdAsync(int id);
    Task<List<ComplexImageResponseDto>> GetByComplexIdAsync(int complexId);
    Task DeleteAsync(int id);
    Task<ComplexImageResponseDto> UploadImageAsync(int complexId, IFormFile file, int userId, string? description = null);
    Task<List<ComplexImageResponseDto>> UploadMultipleImagesAsync(int complexId, List<IFormFile> files, int userId);
    Task<List<ComplexImageResponseDto>> GetImagesByComplexIdAsync(int complexId);
    Task DeleteImageAsync(int imageId, int userId);
    Task SetMainImageAsync(int imageId, int userId);
}