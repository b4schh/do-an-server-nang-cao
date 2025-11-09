using FootballField.API.Dtos;
using FootballField.API.Entities;
using FootballField.API.Services.Interfaces;
using FootballField.API.Repositories.Interfaces;

namespace FootballField.API.Services.Implements;

public class ComplexImageService : IComplexImageService
{
    private readonly IComplexImageRepository _complexImageRepository;

    public ComplexImageService(IComplexImageRepository complexImageRepository)
    {
        _complexImageRepository = complexImageRepository;
    }

    public async Task<ComplexImageResponseDto> CreateAsync(ComplexImageCreateDto dto)
    {
        var complexImage = new ComplexImage
        {
            ComplexId = dto.ComplexId,
            ImageUrl = dto.ImageUrl,
            IsMain = dto.IsMain
        };

        var createdImage = await _complexImageRepository.AddAsync(complexImage);

        return new ComplexImageResponseDto
        {
            Id = createdImage.Id,
            ComplexId = createdImage.ComplexId,
            ImageUrl = createdImage.ImageUrl,
            IsMain = createdImage.IsMain
        };
    }

    public async Task<ComplexImageResponseDto> GetByIdAsync(int id)
    {
        var complexImage = await _complexImageRepository.GetByIdAsync(id);

        if (complexImage == null)
        {
            throw new Exception("Không tìm thấy ảnh!");
        }

        return new ComplexImageResponseDto
        {
            Id = complexImage.Id,
            ComplexId = complexImage.ComplexId,
            ImageUrl = complexImage.ImageUrl,
            IsMain = complexImage.IsMain
        };
    }

    public async Task<List<ComplexImageResponseDto>> GetByComplexIdAsync(int complexId)
    {
        var complexImages = await _complexImageRepository.GetByComplexIdAsync(complexId);

        return complexImages.Select(ci => new ComplexImageResponseDto
        {
            Id = ci.Id,
            ComplexId = ci.ComplexId,
            ImageUrl = ci.ImageUrl,
            IsMain = ci.IsMain
        }).ToList();
    }

    public async Task DeleteAsync(int id)
    {
        var complexImage = await _complexImageRepository.GetByIdAsync(id);

        if (complexImage == null)
        {
            throw new Exception("Không tìm thấy ảnh!");
        }

        await _complexImageRepository.DeleteAsync(complexImage);
    }
}