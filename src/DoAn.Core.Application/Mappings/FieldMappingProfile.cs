using AutoMapper;
using DoAn.Core.Application.DTOs.Field;

namespace DoAn.Core.Application.Mappings;

public class FieldMappingProfile : Profile
{
    public FieldMappingProfile()
    {
        // Field Mapping
        CreateMap<Field, FieldDto>()
            .ForMember(dest => dest.ComplexName, opt => opt.MapFrom(src => src.Complex != null ? src.Complex.Name : null))
            .ForMember(dest => dest.TimeSlotCount, opt => opt.Ignore()); // Will be set separately in service if needed
        
        CreateMap<Field, FieldWithTimeSlotsDto>()
            .ForMember(dest => dest.TimeSlots, opt => opt.MapFrom(src => src.TimeSlots));
        
        CreateMap<CreateFieldDto, Field>();
        
        CreateMap<UpdateFieldDto, Field>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ComplexId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Condition(src => src.IsActive.HasValue));

        // TimeSlot Mapping
        CreateMap<TimeSlot, TimeSlotDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price)) // Explicit mapping
            .ForMember(dest => dest.FieldName, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.ComplexId, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.ComplexName, opt => opt.Ignore()); // Will be set in service
        
        CreateMap<TimeSlot, TimeSlotWithAvailabilityDto>()
            .ForMember(dest => dest.IsBooked, opt => opt.MapFrom(src => false)); // Default false, sẽ được set trong service nếu cần
        
        CreateMap<CreateTimeSlotDto, TimeSlot>();
        
        CreateMap<UpdateTimeSlotDto, TimeSlot>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FieldId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}
