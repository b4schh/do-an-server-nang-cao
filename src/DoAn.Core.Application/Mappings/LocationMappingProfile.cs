using AutoMapper;
using DoAn.Core.Application.DTOs.Location;

namespace DoAn.Core.Application.Mappings;

public class LocationMappingProfile : Profile
{
    public LocationMappingProfile()
    {
        // Location Mapping
        CreateMap<Province, ProvinceDto>();
        CreateMap<Province, ProvinceWithWardsDto>();
        CreateMap<Ward, WardDto>();
    }
}
