using AutoMapper;
using DoAn.Core.Application.DTOs.OwnerSetting;

namespace DoAn.Core.Application.Mappings;

public class OwnerSettingMappingProfile : Profile
{
    public OwnerSettingMappingProfile()
    {
        // OwnerSetting Mapping
        CreateMap<OwnerSetting, OwnerSettingDto>();
        
        CreateMap<CreateOwnerSettingDto, OwnerSetting>();
        
        CreateMap<UpdateOwnerSettingDto, OwnerSetting>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.BankQrCodeUrl, opt => opt.Ignore()); // Handle in service
    }
}
