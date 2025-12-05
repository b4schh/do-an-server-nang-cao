using AutoMapper;
using FootballField.API.Modules.LocationManagement.Dtos;
using FootballField.API.Modules.LocationManagement.Repositories;

namespace FootballField.API.Modules.LocationManagement.Services;

public class WardService : IWardService
{
    private readonly IWardRepository _wardRepository;
    private readonly IMapper _mapper;

    public WardService(IWardRepository wardRepository, IMapper mapper)
    {
        _wardRepository = wardRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<WardDto>> GetWardsByProvinceCodeAsync(int provinceCode)
    {
        var wards = await _wardRepository.GetByProvinceCodeAsync(provinceCode);
        return _mapper.Map<IEnumerable<WardDto>>(wards);
    }

    public async Task<WardDto?> GetWardByCodeAsync(int code)
    {
        var ward = await _wardRepository.GetByCodeAsync(code);
        return ward == null ? null : _mapper.Map<WardDto>(ward);
    }
}
