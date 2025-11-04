using AutoMapper;
using FootballField.API.Entities;
using FootballField.API.Dtos.User;
using FootballField.API.Dtos.Complex;
using FootballField.API.Dtos.Field;

namespace FootballField.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mapping
            CreateMap<User, UserDto>();
            CreateMap<User, UserProfileDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            
            
            CreateMap<Complex, ComplexDto>();
            CreateMap<Complex, ComplexWithFieldsDto>();
            CreateMap<Complex, ComplexFullDetailsDto>()
                .ForMember(dest => dest.Fields, opt => opt.Ignore()); // Ignore vì map thủ công trong Service
            CreateMap<CreateComplexDto, Complex>();
            CreateMap<CreateComplexByOwnerDto, Complex>();
            CreateMap<CreateComplexByAdminDto, Complex>();
            CreateMap<UpdateComplexDto, Complex>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<Field, FieldDto>();
            CreateMap<Field, FieldWithTimeSlotsDto>()
                .ForMember(dest => dest.TimeSlots, opt => opt.MapFrom(src => src.TimeSlots));
            CreateMap<CreateFieldDto, Field>();
            CreateMap<UpdateFieldDto, Field>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ComplexId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}