namespace DoAn.Core.Application.DTOs.Review;

public class GetComplexReviewsResponseDto
{
    public List<ReviewDto> Reviews { get; set; } = new();
    public ReviewStatisticsDto Statistics { get; set; } = new();
}
