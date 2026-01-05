namespace DoAn.Core.Application.DTOs.Review;

public class OwnerReviewDto
{
    public int Id { get; set; }
    public ReviewUserDto User { get; set; } = null!;
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    public List<string> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Helpful { get; set; }
    public bool IsVisible { get; set; }
    public bool IsDeleted { get; set; }

    // Additional info for owner
    public int BookingId { get; set; }
    public int FieldId { get; set; }
    public string FieldName { get; set; } = null!;
    public int ComplexId { get; set; }
    public string ComplexName { get; set; } = null!;
    public int OwnerId { get; set; }
}
