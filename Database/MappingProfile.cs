using AutoMapper;
using FootballField.API.Modules.ComplexManagement.Dtos;
using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Modules.FieldManagement.Dtos;
using FootballField.API.Modules.FieldManagement.Entities;
using FootballField.API.Modules.ReviewManagement.Dtos;
using FootballField.API.Modules.ReviewManagement.Entities;
using FootballField.API.Modules.UserManagement.Dtos;
using FootballField.API.Modules.UserManagement.Entities;

namespace FootballField.API.Database
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleNames, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.RoleNames, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.UserRoles.FirstOrDefault() != null ? src.UserRoles.First().Role.Name : ""));
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // Role Mapping
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.UserCount, opt => opt.Ignore())
                .ForMember(dest => dest.PermissionCount, opt => opt.Ignore());

            // Permission Mapping
            CreateMap<Permission, PermissionDto>();

            // Complex Mapping
            CreateMap<Complex, ComplexDto>();
            CreateMap<Complex, ComplexWithFieldsDto>();
            CreateMap<Complex, ComplexFullDetailsDto>()
                .ForMember(dest => dest.Fields, opt => opt.Ignore()); // Ignore vì map thủ công trong Service
            CreateMap<Complex, ComplexWeeklyDetailsDto>()
                .ForMember(dest => dest.Fields, opt => opt.Ignore()); // Ignore vì map thủ công trong Service
            CreateMap<CreateComplexDto, Complex>();
            CreateMap<CreateComplexByOwnerDto, Complex>();
            CreateMap<CreateComplexByAdminDto, Complex>();
            CreateMap<UpdateComplexDto, Complex>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // Field Mapping
            CreateMap<Field, FieldDto>();
            CreateMap<Field, FieldWithTimeSlotsDto>()
                .ForMember(dest => dest.TimeSlots, opt => opt.MapFrom(src => src.TimeSlots));
            CreateMap<CreateFieldDto, Field>();
            CreateMap<UpdateFieldDto, Field>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ComplexId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // Timeslot Mapping
            CreateMap<TimeSlot, TimeSlotDto>();
            CreateMap<TimeSlot, TimeSlotWithAvailabilityDto>()
                .ForMember(dest => dest.IsBooked, opt => opt.MapFrom(src => false)); // Default false, sẽ được set trong service nếu cần
            CreateMap<CreateTimeSlotDto, TimeSlot>();
            CreateMap<UpdateTimeSlotDto, TimeSlot>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FieldId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // Review Mapping - Manual mapping in service for complex scenarios
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.User, opt => opt.Ignore()) // Map trong service
                .ForMember(dest => dest.Images, opt => opt.Ignore()) // Map trong service
                .ForMember(dest => dest.Helpful, opt => opt.MapFrom(src => src.HelpfulVotes.Count));

            CreateMap<CreateReviewDto, Review>()
                .ForMember(dest => dest.Images, opt => opt.Ignore()); // Handle trong service
                
            CreateMap<UpdateReviewDto, Review>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest.IsVisible, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
        }
    }
}