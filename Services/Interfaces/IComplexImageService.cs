using FootballField.API.Dtos;

namespace FootballField.API.Services.Interfaces;

public interface IComplexImageService
{
    Task<ComplexImageResponseDto> CreateAsync(ComplexImageCreateDto dto);
    Task<ComplexImageResponseDto> GetByIdAsync(long id);
    Task<List<ComplexImageResponseDto>> GetByComplexIdAsync(int complexId);
    Task DeleteAsync(long id);
}