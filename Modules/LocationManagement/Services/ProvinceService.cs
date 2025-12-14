using AutoMapper;
using FootballField.API.Modules.LocationManagement.Dtos;
using FootballField.API.Modules.LocationManagement.Repositories;

namespace FootballField.API.Modules.LocationManagement.Services;

public class ProvinceService : IProvinceService
{
    private readonly IProvinceRepository _provinceRepository;
    private readonly IMapper _mapper;

    public ProvinceService(IProvinceRepository provinceRepository, IMapper mapper)
    {
        _provinceRepository = provinceRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProvinceDto>> GetAllProvincesAsync()
    {
        var provinces = await _provinceRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProvinceDto>>(provinces);
    }

    public async Task<ProvinceDto?> GetProvinceByCodeAsync(int code)
    {
        var province = await _provinceRepository.GetByCodeAsync(code);
        return province == null ? null : _mapper.Map<ProvinceDto>(province);
    }

    public async Task<ProvinceWithWardsDto?> GetProvinceWithWardsByCodeAsync(int code)
    {
        var province = await _provinceRepository.GetByCodeWithWardsAsync(code);
        return province == null ? null : _mapper.Map<ProvinceWithWardsDto>(province);
    }
}
