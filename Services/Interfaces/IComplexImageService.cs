using FootballField.API.Dtos;

namespace FootballField.API.Services.Interfaces;

public interface IComplexImageService
{
    Task<ComplexImageResponseDto> CreateAsync(ComplexImageCreateDto dto);
    Task<ComplexImageResponseDto> GetByIdAsync(int id);
    Task<List<ComplexImageResponseDto>> GetByComplexIdAsync(int complexId);
    Task DeleteAsync(int id);
}