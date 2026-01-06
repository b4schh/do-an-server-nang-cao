using AutoMapper;
using DoAn.Core.Application.DTOs.Booking;
using DoAn.Core.Domain.Entities;

namespace DoAn.Core.Application.Mappings;

public class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.Name : null))
            .ForMember(dest => dest.ComplexId, opt => opt.MapFrom(src => src.Field != null ? src.Field.ComplexId : 0))
            .ForMember(dest => dest.ComplexName, opt => opt.MapFrom(src => src.Field != null && src.Field.Complex != null ? src.Field.Complex.Name : null))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? $"{src.Customer.LastName} {src.Customer.FirstName}".Trim() : null))
            .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Phone : null))
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner != null ? $"{src.Owner.LastName} {src.Owner.FirstName}".Trim() : null))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.TimeSlot != null ? src.TimeSlot.StartTime : (TimeSpan?)null))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.TimeSlot != null ? src.TimeSlot.EndTime : (TimeSpan?)null))
            .ForMember(dest => dest.BookingStatusText, opt => opt.MapFrom(src => GetStatusText(src.BookingStatus)))
            .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? $"{src.ApprovedByUser.LastName} {src.ApprovedByUser.FirstName}".Trim() : null))
            .ForMember(dest => dest.CancelledByName, opt => opt.MapFrom(src => src.CancelledByUser != null ? $"{src.CancelledByUser.LastName} {src.CancelledByUser.FirstName}".Trim() : null));

        CreateMap<CreateBookingDto, Booking>();
    }

    private static string GetStatusText(BookingStatus status)
    {
        return status switch
        {
            BookingStatus.Pending => "Chờ thanh toán",
            BookingStatus.WaitingForApproval => "Chờ duyệt",
            BookingStatus.Confirmed => "Đã xác nhận",
            BookingStatus.Rejected => "Bị từ chối",
            BookingStatus.Cancelled => "Đã hủy",
            BookingStatus.Completed => "Hoàn thành",
            BookingStatus.Expired => "Hết hạn",
            BookingStatus.NoShow => "Không đến",
            _ => "Không xác định"
        };
    }
}
