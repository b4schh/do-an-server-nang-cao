namespace DoAn.Core.Application.DTOs.Recommendation;

public class RecommendationResponse
{
    public string RecommendationType { get; set; } = null!;
    public List<FieldRecommendationDto> Fields { get; set; } = new();
    public string? Message { get; set; }
}
