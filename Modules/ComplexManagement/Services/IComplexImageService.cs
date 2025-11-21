using FootballField.API.Modules.ComplexManagement.Dtos;

namespace FootballField.API.Modules.ComplexManagement.Services;

public interface IComplexImageService
{
    Task<ComplexImageResponseDto> CreateAsync(ComplexImageCreateDto dto);
    Task<ComplexImageResponseDto> GetByIdAsync(int id);
    Task<List<ComplexImageResponseDto>> GetByComplexIdAsync(int complexId);
    Task DeleteAsync(int id);
}