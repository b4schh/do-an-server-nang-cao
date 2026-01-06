namespace DoAn.Core.Application.DTOs.Recommendation;

public class RecommendationResponse
{
    public string RecommendationType { get; set; } = null!;
    public List<ComplexRecommendationDto> Complexes { get; set; } = new();
    public string? Message { get; set; }
}

/// <summary>
/// [DEPRECATED] Field-level response - Kept for backward compatibility
/// </summary>
[Obsolete("Use RecommendationResponse with Complexes instead")]
public class FieldRecommendationResponse
{
    public string RecommendationType { get; set; } = null!;
    public List<FieldRecommendationDto> Fields { get; set; } = new();
    public string? Message { get; set; }
}
