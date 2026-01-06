using DoAn.Core.Application.DTOs.Field;

namespace DoAn.Core.Application.DTOs.Complex;

public class AdminComplexDetailDto
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = null!;
    public string OwnerEmail { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Street { get; set; }
    public string? Ward { get; set; }
    public string? Province { get; set; }
    public string? Phone { get; set; }
    public TimeSpan? OpeningTime { get; set; }
    public TimeSpan? ClosingTime { get; set; }
    public string? Description { get; set; }
    public ComplexStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Rating & Reviews
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // Images
    public IEnumerable<ComplexImageResponseDto> Images { get; set; } = new List<ComplexImageResponseDto>();

    // Fields
    public IEnumerable<FieldDto> Fields { get; set; } = new List<FieldDto>();
    public int FieldCount { get; set; }
}
