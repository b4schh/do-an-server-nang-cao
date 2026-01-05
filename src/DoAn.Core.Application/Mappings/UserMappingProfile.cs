using AutoMapper;
using DoAn.Core.Application.DTOs.User;

namespace DoAn.Core.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
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
    }
}
