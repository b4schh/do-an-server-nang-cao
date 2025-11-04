using FootballField.API.Dtos;
using FootballField.API.Entities;
using FootballField.API.Services.Interfaces;
using FootballField.API.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Services.Implements;

public class ComplexImageService : IComplexImageService
{
    private readonly ApplicationDbContext _context;

    public ComplexImageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ComplexImageResponseDto> CreateAsync(ComplexImageCreateDto dto)
    {
        var complexImage = new ComplexImage
        {
            ComplexId = dto.ComplexId,
            ImageUrl = dto.ImageUrl,
            IsMain = dto.IsMain
        };

        _context.ComplexImages.Add(complexImage);
        await _context.SaveChangesAsync();

        return new ComplexImageResponseDto
        {
            Id = complexImage.Id,
            ComplexId = complexImage.ComplexId,
            ImageUrl = complexImage.ImageUrl,
            IsMain = complexImage.IsMain
        };
    }

    public async Task<ComplexImageResponseDto> GetByIdAsync(long id)
    {
        var complexImage = await _context.ComplexImages
            .FirstOrDefaultAsync(ci => ci.Id == id);

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
        var complexImages = await _context.ComplexImages
            .Where(ci => ci.ComplexId == complexId)
            .ToListAsync();

        return complexImages.Select(ci => new ComplexImageResponseDto
        {
            Id = ci.Id,
            ComplexId = ci.ComplexId,
            ImageUrl = ci.ImageUrl,
            IsMain = ci.IsMain
        }).ToList();
    }

    public async Task DeleteAsync(long id)
    {
        var complexImage = await _context.ComplexImages
            .FirstOrDefaultAsync(ci => ci.Id == id);

        if (complexImage == null)
        {
            throw new Exception("Không tìm thấy ảnh!");
        }

        _context.ComplexImages.Remove(complexImage);
        await _context.SaveChangesAsync();
    }
}