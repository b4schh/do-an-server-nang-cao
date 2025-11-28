namespace FootballField.API.Modules.ReviewManagement.Dtos;

public class GetComplexReviewsResponseDto
{
    public List<ReviewDto> Reviews { get; set; } = new();
    public ReviewStatisticsDto Statistics { get; set; } = new();
}
