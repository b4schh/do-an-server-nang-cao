namespace DoAn.Core.Application.DTOs.Recommendation;

public class NewUserRecommendationRequest
{
    public string? Province { get; set; }
    public string? Ward { get; set; }
    public int TopK { get; set; } = 10;
}

public class PersonalizedRecommendationRequest
{
    public int UserId { get; set; }
    public int TopK { get; set; } = 10;
    public string? Province { get; set; }
}

public class SimilarFieldsRequest
{
    public int FieldId { get; set; }
    public int TopK { get; set; } = 10;
}
