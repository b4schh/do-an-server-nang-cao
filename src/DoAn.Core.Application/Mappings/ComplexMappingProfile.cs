using AutoMapper;
using DoAn.Core.Application.DTOs.Complex;

namespace DoAn.Core.Application.Mappings;

public class ComplexMappingProfile : Profile
{
    public ComplexMappingProfile()
    {
        // Complex Mapping
        CreateMap<Complex, ComplexDto>()
            .ForMember(dest => dest.MainImageUrl, opt => opt.MapFrom(src => src.ComplexImages.FirstOrDefault(img => img.IsMain) != null ? src.ComplexImages.First(img => img.IsMain).ImageUrl : null))
            .ForMember(dest => dest.FieldCount, opt => opt.MapFrom(src => src.Fields.Count(f => !f.IsDeleted)));
        
        CreateMap<Complex, ComplexWithFieldsDto>();
        
        CreateMap<Complex, ComplexFullDetailsDto>()
            .ForMember(dest => dest.Fields, opt => opt.Ignore()); // Ignore vì map thủ công trong Service
        
        CreateMap<Complex, ComplexWeeklyDetailsDto>()
            .ForMember(dest => dest.Fields, opt => opt.Ignore()); // Ignore vì map thủ công trong Service

        // Admin detail mapping - includes owner info, rating, reviews, images
        CreateMap<Complex, AdminComplexDetailDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.Ignore())  // Set in service
            .ForMember(dest => dest.OwnerEmail, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.AverageRating, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.ReviewCount, opt => opt.Ignore())   // Set in service
            .ForMember(dest => dest.Images, opt => opt.Ignore())        // Set in service
            .ForMember(dest => dest.FieldCount, opt => opt.MapFrom(src => src.Fields.Count(f => !f.IsDeleted)));
        
        // ComplexImage mappings
        CreateMap<ComplexImage, ComplexImageResponseDto>();
        CreateMap<ComplexImageCreateDto, ComplexImage>();
        
        CreateMap<CreateComplexDto, Complex>();
        
        CreateMap<CreateComplexByOwnerDto, Complex>();
        
        CreateMap<CreateComplexByAdminDto, Complex>();
        
        CreateMap<UpdateComplexDto, Complex>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
