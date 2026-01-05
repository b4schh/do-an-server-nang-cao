using AutoMapper;
using DoAn.Core.Application.DTOs.Review;

namespace DoAn.Core.Application.Mappings;

public class ReviewMappingProfile : Profile
{
    public ReviewMappingProfile()
    {
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
