using AutoMapper;
using DoAn.Core.Application.DTOs.SystemConfig;

namespace DoAn.Core.Application.Mappings;

public class SystemConfigMappingProfile : Profile
{
    public SystemConfigMappingProfile()
    {
        // SystemConfig Mapping
        CreateMap<SystemConfig, SystemConfigDto>();
        
        CreateMap<UpdateSystemConfigDto, SystemConfig>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ConfigKey, opt => opt.Ignore())
            .ForMember(dest => dest.DataType, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}
