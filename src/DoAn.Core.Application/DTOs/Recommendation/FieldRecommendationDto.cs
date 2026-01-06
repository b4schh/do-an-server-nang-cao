namespace DoAn.Core.Application.DTOs.Recommendation;

public class FieldRecommendationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int ComplexId { get; set; }
    public string ComplexName { get; set; } = null!;
    public string? Province { get; set; }
    public string? Ward { get; set; }
    public decimal? Price { get; set; }
    public int FieldSize { get; set; }
    public string? SurfaceType { get; set; }
    public double? AverageRating { get; set; }
    public int BookingCount { get; set; }
    public double SimilarityScore { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}
