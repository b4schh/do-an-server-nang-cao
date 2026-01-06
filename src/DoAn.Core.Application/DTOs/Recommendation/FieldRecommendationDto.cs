namespace DoAn.Core.Application.DTOs.Recommendation;

/// <summary>
/// DTO for Complex-level recommendation (recommended approach)
/// Gợi ý ở level Complex vì customer tương tác với Complex, không phải Field riêng lẻ
/// </summary>
public class ComplexRecommendationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Province { get; set; }
    public string? Ward { get; set; }
    public string? Street { get; set; }
    public string? Phone { get; set; }
    
    /// <summary>
    /// Khoảng giá: "100,000đ - 500,000đ"
    /// </summary>
    public string PriceRange { get; set; } = null!;
    
    /// <summary>
    /// Các loại sân có sẵn: ["Sân 5", "Sân 7", "Sân 11"]
    /// </summary>
    public List<string> FieldTypes { get; set; } = new();
    
    /// <summary>
    /// Các loại mặt sân: ["Cỏ tự nhiên", "Cỏ nhân tạo"]
    /// </summary>
    public List<string> SurfaceTypes { get; set; } = new();
    
    /// <summary>
    /// Số lượng sân có sẵn
    /// </summary>
    public int TotalFields { get; set; }
    
    public double AverageRating { get; set; }
    public int TotalBookings { get; set; }
    public double SimilarityScore { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string? OpeningTime { get; set; }
    public string? ClosingTime { get; set; }
}

/// <summary>
/// [DEPRECATED] Field-level DTO - Kept for backward compatibility
/// Khuyến nghị dùng ComplexRecommendationDto thay thế
/// </summary>
[Obsolete("Use ComplexRecommendationDto instead. Customer interacts with Complex, not individual Fields.")]
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
